using System.Collections.ObjectModel;
using System.Linq;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Entities;
using SelectedTextSpeach.Data.Models.Repositories;

namespace SelectedTextSpeach.Data.Models
{
    public interface IStoryModel
    {
        Story InitialStory { get; }
        ReactivePropertySlim<Story> CurrentStory { get; }
        ObservableCollection<Story> AllStories { get; }
        void ChangeCurrentStoryByTitle(string title);
    }

    public class HarryPotterStoryModel : IStoryModel
    {
        private readonly IStoryRepository repository;
        public Story InitialStory { get; }
        public ReactivePropertySlim<Story> CurrentStory { get; }
        public ObservableCollection<Story> AllStories { get; }

        public HarryPotterStoryModel()
        {
            repository = new StoryRepostiory();
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync().Result;
            foreach (var (order, titleKey, contentKey) in ApplicationSettings.HarryPotterStoryTextResources)
            {
                repository.Add(resourceLoader.GetString(titleKey), resourceLoader.GetString(contentKey));
            }
            AllStories = new ObservableCollection<Story>(repository.All());

            InitialStory = AllStories.FirstOrDefault();

            CurrentStory = new ReactivePropertySlim<Story>
            {
                Value = InitialStory
            };
        }

        public void ChangeCurrentStoryByTitle(string title)
        {
            var current = repository.Get(title);
            if (current != null)
            {
                CurrentStory.Value = current;
            }
        }
    }
}
