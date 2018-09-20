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
        Story[] AllStories { get; }
        void ChangeCurrentStoryByTitleHash(int titleHash);
    }

    public class HarryPotterStoryModel : IStoryModel
    {
        private readonly IStoryRepository repository;
        public Story InitialStory { get; }
        public ReactivePropertySlim<Story> CurrentStory { get; }
        public Story[] AllStories { get; }

        public HarryPotterStoryModel()
        {

            repository = new StoryRepostiory();
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync().Result;
            foreach (var (order, titleKey, contentKey) in ApplicationSettings.HarryPotterStoryTextResources)
            {
                repository.Add(resourceLoader.GetString(titleKey), resourceLoader.GetString(contentKey));
            }
            AllStories = repository.All();
            InitialStory = AllStories.FirstOrDefault();

            CurrentStory = new ReactivePropertySlim<Story>
            {
                Value = InitialStory
            };
        }

        public void ChangeCurrentStoryByTitleHash(int titleHash)
        {
            var current = repository.Get(titleHash);
            CurrentStory.Value = current;
        }
    }
}
