using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SelectedTextSpeach.Models.Entities;
using SelectedTextSpeach.Models.UseCases;
using Windows.ApplicationModel.DataTransfer;

namespace SelectedTextSpeach.ViewModels
{
    public class ChoiceArtifactViewModel : IDisposable
    {
        private IBlobArtifactSummary usecase = new BlobArtifactUseCase();
        private CompositeDisposable disposable = new CompositeDisposable();
        private DataPackage dataPackage = new DataPackage();

        public ReactiveProperty<string> StorageConnectionInput { get; }
        public ReactiveProperty<string> StorageContainerInput { get; }

        public ReactiveProperty<bool> IsCheckBoxChecked { get; } = new ReactiveProperty<bool>(true);
        public AsyncReactiveCommand OnClickCheckBlobCommand { get; }
        public ReactiveProperty<bool> IsCheckBlobCompleted { get; } = new ReactiveProperty<bool>();
        public ReactiveCommand OnClickCancelBlobCommand { get; }
        public ReactiveProperty<string> BlobResult { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<bool> ComboBoxEnabled { get; set; }

        public ObservableCollection<IArtifactEntity> Projects { get; } = new ObservableCollection<IArtifactEntity>();
        public ReactiveProperty<IArtifactEntity> SelectedProject { get; } = new ReactiveProperty<IArtifactEntity>();
        public ReactiveCollection<IBranchArtifactEntity> Branches { get; }
        public ReactiveProperty<IBranchArtifactEntity> SelectedBranch { get; } = new ReactiveProperty<IBranchArtifactEntity>();
        public ReactiveCollection<IArtifactDetailEntity> Artifacts { get; }
        public ReactiveProperty<IArtifactDetailEntity> SelectedArtifact { get; } = new ReactiveProperty<IArtifactDetailEntity>();
        public ReactiveProperty<string> ArtifactName { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> ArtifactCaption { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> ArtifactUrl { get; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> CopyButtonContent { get; }
        public ReactiveProperty<bool> CopyButtonEnabled { get; }
        public ReactiveCommand OnClickCopyCommand { get; }

        public AsyncReactiveCommand OnClickDownloadCommand { get; }
        public ReactiveProperty<string> DownloadStatus { get; } = new ReactiveProperty<string>();
        public AsyncReactiveCommand OnClickOpenDownloadFolderCommand { get; } = new AsyncReactiveCommand();

        public ChoiceArtifactViewModel()
        {
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync("resourceFile").Result;
            var blobConnectionString = resourceLoader.GetString("azure_storage_blob_connectionstring");
            var containerName = resourceLoader.GetString("container_name");

            StorageConnectionInput = new ReactiveProperty<string>(blobConnectionString);
            StorageContainerInput = new ReactiveProperty<string>(containerName);

            // Copy Button
            CopyButtonContent = new ReactiveProperty<string>("Copy");
            CopyButtonEnabled = ArtifactUrl.Select(x => !string.IsNullOrWhiteSpace(x)).ToReactiveProperty();
            OnClickCopyCommand = CopyButtonEnabled.ToReactiveCommand();
            OnClickCopyCommand
                .Do(_ => ClipboardHelper.CopyToClipboard(ArtifactUrl.Value))
                .SelectMany(x => TemporaryDisableCopyButtonAsObservable())
                .Subscribe()
                .AddTo(disposable);

            // Download Button
            usecase.DownloadStatus.Subscribe(x => DownloadStatus.Value = x).AddTo(disposable);
            OnClickDownloadCommand = CopyButtonEnabled.ToAsyncReactiveCommand();
            OnClickDownloadCommand
                .Subscribe(async _ => await usecase.DownloadHoloLensPackagesAsync(blobConnectionString, containerName, SelectedArtifact.Value.Name, SelectedArtifact.Value.Size, SelectedArtifact.Value.FileName))
                .AddTo(disposable);

            // OpenFolder Button
            OnClickOpenDownloadFolderCommand.Subscribe(_ => usecase.OpenFolderAsync()).AddTo(disposable);

            // Initialize by obtain artifact informations
            usecase.Artifacts
                .Where(x => x != null)
                .Do(x =>
                {
                    Projects.Add(x);
                    BlobResult.Value = $"Found {Projects.Count} projects.";
                })
                .Subscribe()
                .AddTo(disposable);
            usecase.RequestFailedMessage
                .Do(x => BlobResult.Value = x)
                .Subscribe()
                .AddTo(disposable);

            // Blob Download
            ComboBoxEnabled = Projects.CollectionChangedAsObservable().Any().ToReactiveProperty();
            OnClickCheckBlobCommand = IsCheckBoxChecked.ToAsyncReactiveCommand();
            OnClickCheckBlobCommand.Subscribe(async _ =>
            {
                var task = usecase.RequestHoloLensPackagesAsync(StorageConnectionInput.Value, StorageContainerInput.Value);
                Projects.Clear();
                Branches?.Clear();
                Artifacts?.Clear();
                BlobResult.Value = "Trying obtain project infomations.";
                await task;
                IsCheckBlobCompleted.Value = true;
            })
            .AddTo(disposable);
            OnClickCancelBlobCommand = IsCheckBoxChecked.Select(x => !x).ToReactiveCommand();
            OnClickCancelBlobCommand.Subscribe(_ => usecase.CancelRequest()).AddTo(disposable);

            // Update Collection with Clear existing collection when selected.
            Branches = SelectedProject.Where(x => x != null)
                .Do(_ => Branches?.Clear())
                .Do(_ => Artifacts?.Clear())
                .SelectMany(x => usecase.GetArtifactCache(x.Project))
                .ToReactiveCollection();
            Artifacts = SelectedBranch.Where(x => x != null)
                .Do(x => Artifacts?.Clear())
                .SelectMany(x => usecase.GetArtifactCache(SelectedProject.Value?.Project, x.Branch))
                .ToReactiveCollection();
            SelectedArtifact
                .Where(x => x != null)
                .Do(x =>
                {
                    ArtifactName.Value = x.Name;
                    ArtifactCaption.Value = $"(Size: {x.Size}, MD5: {x.MD5}, LeaseState: {x.LeaseState})";
                    ArtifactUrl.Value = x.Uri.AbsoluteUri;
                })
                .ToReactiveProperty();

            // Next action
            // TODO : Cancel Download
            // TODO : Multiple Download?
        }

        private IObservable<Unit> TemporaryDisableCopyButtonAsObservable()
        {
            // Change ButtonContent a while
            return Observable.Start(() =>
            {
                CopyButtonContent.Value = "Copied!!";
                CopyButtonEnabled.Value = false;
            })
            .Delay(TimeSpan.FromMilliseconds(500))
            .Do(__ =>
            {
                CopyButtonContent.Value = "Copy";
                CopyButtonEnabled.Value = true;
            })
            .ToUnit();
        }

        public void Dispose()
        {
            disposable.Dispose();

        }
    }
}
