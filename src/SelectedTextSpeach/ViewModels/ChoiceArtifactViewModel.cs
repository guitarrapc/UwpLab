using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SelectedTextSpeach.Models.Entities;
using SelectedTextSpeach.Models.UseCases;
using WinRTXamlToolkit.Tools;

namespace SelectedTextSpeach.ViewModels
{
    public class ChoiceArtifactViewModel : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IBlobArtifact usecase = new BlobArtifactUseCase();
        private CompositeDisposable disposable = new CompositeDisposable();

        public ReactiveProperty<string> StorageConnectionInput { get; }
        public ReactiveProperty<string> StorageContainerInput { get; }

        //TODO: Should change Data Structure.
        public ObservableCollection<IArtifactEntity> Projects { get; } = new ObservableCollection<IArtifactEntity>();
        public ReactiveProperty<IArtifactEntity> SelectedProject { get; } = new ReactiveProperty<IArtifactEntity>();
        public ReactiveCollection<IBranchArtifactEntity> Branches { get; }
        public ReactiveProperty<IBranchArtifactEntity> SelectedBranch { get; } = new ReactiveProperty<IBranchArtifactEntity>();
        public ReactiveCollection<IArtifactDetailEntity> Artifacts { get; }
        public ReactiveProperty<IArtifactDetailEntity> SelectedArtifact { get; } = new ReactiveProperty<IArtifactDetailEntity>();

        public ChoiceArtifactViewModel()
        {
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync("resourceFile").Result;
            var blobConnectionString = resourceLoader.GetString("azure_storage_blob_connectionstring");
            var containerName = resourceLoader.GetString("container_name");

            StorageConnectionInput = new ReactiveProperty<string>(blobConnectionString);
            StorageContainerInput = new ReactiveProperty<string>(containerName);

            // Initialize by obtain artifact informations
            usecase.Artifacts
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    x.ForEach(y => Projects.Add(y));
                })
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

            // Collection's Initial Selection
            Branches.CollectionChangedAsObservable()
                .Select(x => x.NewItems)
                .Where(x => x != null)
                .Select(x => x.ToList<IBranchArtifactEntity>())
                .Where(x => x.Any())
                .Subscribe(x => SelectedBranch.Value = x.First())
                .AddTo(disposable);

            // Next action
            //TODO: Enable Download
            //TODO: Set Download Path
            //TODO: Show Download Detail
            SelectedArtifact.Subscribe().AddTo(disposable);
        }

        public async void OpenBlob()
        {
            await usecase.RequestHoloLensPackagesAsync(StorageConnectionInput.Value, StorageContainerInput.Value);
        }

        public void Dispose()
        {
            disposable.Dispose();

        }
    }
}
