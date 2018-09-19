using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reactive.Bindings;
using SelectedTextSpeach.Data.Entities;

namespace SelectedTextSpeach.Data.Models
{
    public interface IStoryRepository
    {
        Story Get(int key);
        Story Get(string title);
        Story[] All();
        void Remove(int key);
        void Remove(string title);
        void Clear();

    }
    public class StoryModel : IStoryRepository
    {
        public ReactiveProperty<Story> CurrentStory { get; set; }

        private readonly List<Story> stories = new List<Story>();
        private ConcurrentDictionary<int, string> StoryTitleReference { get; } = new ConcurrentDictionary<int, string>();

        public StoryModel()
        {
            var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync().Result;
            foreach (var (order, titleKey, contentKey) in ApplicationSettings.StoryTextResourceMatching)
            {
                stories.Add(new Story(resourceLoader.GetString(titleKey), resourceLoader.GetString(contentKey)));
                // Create reference by Story Title
                var title = resourceLoader.GetString(titleKey);
                StoryTitleReference.TryAdd(title.GetHashCode(), resourceLoader.GetString(titleKey));
            }
        }

        public Story Get(int key)
        {
            var title = StoryTitleReference[key];
            return Get(title);
        }

        public Story Get(string title)
        {
            return stories.FirstOrDefault(x => x.Title == title);
        }

        public Story[] All()
        {
            return stories.ToArray();
        }

        public Story FirstOrDefailt(Story storyDefault = null)
        {
            var result = stories.FirstOrDefault();
            if (result == null)
            {
                return storyDefault;
            }
            return result;
        }

        public void Remove(int key)
        {
            var title = StoryTitleReference[key];
            Remove(title);
        }

        public void Remove(string title)
        {
            var index = stories.FindIndex(0, 1, x => x.Title == title);
            stories.RemoveAt(index);
        }

        public void Clear()
        {
            stories.Clear();
        }
    }
}
