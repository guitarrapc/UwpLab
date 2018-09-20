using System.Collections.ObjectModel;
using System.Linq;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Entities;
using SelectedTextSpeach.Data.Models.Repositories;

namespace SelectedTextSpeach.Data.Models
{
    public interface IStoryModel
    {
        StoryEntity InitialStory { get; }
        ReactivePropertySlim<StoryEntity> CurrentStory { get; }
        ObservableCollection<StoryEntity> AllStories { get; }
        void ChangeCurrentStoryByTitle(string title);
    }

    public class HarryPotterStoryModel : IStoryModel
    {
        private readonly IStoryRepository repository;
        public StoryEntity InitialStory { get; }
        public ReactivePropertySlim<StoryEntity> CurrentStory { get; }
        public ObservableCollection<StoryEntity> AllStories { get; }

        public HarryPotterStoryModel()
        {
            repository = new StoryRepostiory();
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync().Result;
            foreach (var (order, titleKey, contentKey) in ApplicationSettings.HarryPotterStoryTextResources)
            {
                repository.Add(resourceLoader.GetString(titleKey), resourceLoader.GetString(contentKey));
            }
            AllStories = new ObservableCollection<StoryEntity>(repository.All());

            InitialStory = AllStories.FirstOrDefault();

            CurrentStory = new ReactivePropertySlim<StoryEntity>
            {
                Value = InitialStory
            };
        }

        /// <summary>
        /// CurrentStory will be notify from <see cref="CurrentStory"/> Reactive Property
        /// </summary>
        /// <param name="title"></param>
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
