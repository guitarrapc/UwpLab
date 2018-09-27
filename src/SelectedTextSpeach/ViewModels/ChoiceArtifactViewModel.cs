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
using WinRTXamlToolkit.Tools;

namespace SelectedTextSpeach.ViewModels
{
    public class ChoiceArtifactViewModel : IDisposable
    {
        private IBlobArtifact usecase = new BlobArtifactUseCase();
        private CompositeDisposable disposable = new CompositeDisposable();
        private DataPackage dataPackage = new DataPackage();

        public ReactiveProperty<string> StorageConnectionInput { get; }
        public ReactiveProperty<string> StorageContainerInput { get; }

        public ReactiveProperty<bool> IsCheckBoxChecked { get; } = new ReactiveProperty<bool>(true);
        public AsyncReactiveCommand CheckBlobCommand { get; }

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
        public ReactiveCommand OnClickCopyButton { get; }

        public ChoiceArtifactViewModel()
        {
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync("resourceFile").Result;
            var blobConnectionString = resourceLoader.GetString("azure_storage_blob_connectionstring");
            var containerName = resourceLoader.GetString("container_name");

            StorageConnectionInput = new ReactiveProperty<string>(blobConnectionString);
            StorageContainerInput = new ReactiveProperty<string>(containerName);

            // Copy Button
            CopyButtonContent = new ReactiveProperty<string>("Copy");
            CopyButtonEnabled = ArtifactUrl.Delay(TimeSpan.FromSeconds(1))
                .Select(x => !string.IsNullOrWhiteSpace(x))
                .ToReactiveProperty();
            OnClickCopyButton = CopyButtonEnabled.ToReactiveCommand();
            OnClickCopyButton
                .Do(_ => ClipboardHelper.CopyToClipboard(ArtifactUrl.Value))
                .SelectMany(x => TemporaryDisableCopyButtonAsObservable())
                .Subscribe()
                .AddTo(disposable);

            // Initialize by obtain artifact informations
            usecase.Artifacts
                .Where(x => x != null)
                .Do(x => x.ForEach(y => Projects.Add(y)))
                .Subscribe()
                .AddTo(disposable);

            // Blob Download
            CheckBlobCommand = IsCheckBoxChecked.ToAsyncReactiveCommand();
            CheckBlobCommand
                .Subscribe(async _ => await usecase.RequestHoloLensPackagesAsync(StorageConnectionInput.Value, StorageContainerInput.Value))
                .AddTo(disposable);

            // Update Collection with Clear existing collection when selected.
            Branches = SelectedProject.Where(x => x != null)
                .Do(_ => Branches?.Clear())
                .Do(_ => Artifacts?.Clear())
                .SelectMany(x => usecase.GetArtifactCache(x.Project))
                .ToReactiveCollection();
            Artifacts = SelectedBranch.Where(x => x != null)
                .Do(_ => Artifacts?.Clear())
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

            // Collection's Initial Selection
            Branches.CollectionChangedAsObservable()
                .Select(x => x.NewItems)
                .Where(x => x != null)
                .Select(x => x.ToList<IBranchArtifactEntity>())
                .Where(x => x.Any())
                .Do(x => SelectedBranch.Value = x.First())
                .Subscribe()
                .AddTo(disposable);

            // Next action
            //TODO: Enable Download
            //TODO: Set Download Path
            //TODO: Show Download Detail
        }

        private IObservable<Unit> TemporaryDisableCopyButtonAsObservable()
        {
            // Change ButtonContent a while
            return Observable.Start(() =>
            {
                CopyButtonContent.Value = "Copied!!";
                CopyButtonEnabled.Value = false;
            })
            .Delay(TimeSpan.FromSeconds(1))
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
