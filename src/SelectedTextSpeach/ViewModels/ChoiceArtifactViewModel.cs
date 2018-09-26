using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SelectedTextSpeach.Data.Entities;
using SelectedTextSpeach.Models.Usecase;
using WinRTXamlToolkit.Tools;

namespace SelectedTextSpeach.ViewModels
{
    public class ChoiceArtifactViewModel : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IBlobArtifact model = new BlobArtifactUsecase();
        private CompositeDisposable disposable = new CompositeDisposable();

        public ReactiveProperty<string> StorageConnectionInput { get; }
        public ReactiveProperty<string> StorageContainerInput { get; }

        //TODO: Should change Data Structure.
        public ObservableCollection<string> Projects { get; } = new ObservableCollection<string>();
        public ReactiveProperty<string> SelectedProject { get; } = new ReactiveProperty<string>();
        public ObservableCollection<string> Branches { get; } = new ObservableCollection<string>();
        public ReactiveProperty<string> SelectedBranch { get; } = new ReactiveProperty<string>();
        public ObservableCollection<BlobArtifactEntity> Artifacts { get; } = new ObservableCollection<BlobArtifactEntity>();
        public ReactiveProperty<BlobArtifactEntity> SelectedArtifact { get; } = new ReactiveProperty<BlobArtifactEntity>();

        public ChoiceArtifactViewModel()
        {
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync("resourceFile").Result;
            var blobConnectionString = resourceLoader.GetString("azure_storage_blob_connectionstring");
            var containerName = resourceLoader.GetString("container_name");

            StorageConnectionInput = new ReactiveProperty<string>(blobConnectionString);
            StorageContainerInput = new ReactiveProperty<string>(containerName);

            model.Artifacts
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    x.Select(y => y.Project)
                        .Distinct()
                        .ForEach(item => Projects.Add(item));
                })
                .AddTo(disposable);

            SelectedArtifact.Subscribe().AddTo(disposable);
        }

        public async void OpenBlob()
        {
            await model.RequestHoloLensPackagesAsync(StorageConnectionInput.Value, StorageContainerInput.Value);
        }

        public async void ProjectComboBox_SelectionChanged()
        {
            Branches.Clear();
            Artifacts.Clear();
            model.GetInfo()
                .Where(x => x.Project == SelectedProject.Value)
                .Select(x => x.Branch)
                .Distinct()
                .ForEach(item => Branches.Add(item));
        }

        public async void BranchComboBox_SelectionChanged()
        {
            Artifacts.Clear();
            model.GetInfo()
                .Where(x => x.Project == SelectedProject.Value)
                .Where(x => x.Branch == SelectedBranch.Value)
                .ForEach(item => Artifacts.Add(item));
        }

        public async void ArtifactComboBox_SelectionChanged()
        {
            var hoge = SelectedArtifact;
        }

        public void Dispose()
        {
            disposable.Dispose();

        }
    }
}
