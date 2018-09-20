using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SelectedTextSpeach.Data.Entities;

namespace SelectedTextSpeach.Data.Models.Repositories
{
    public interface IStoryRepository
    {
        void Add(string title, string content);
        StoryEntity Get(string title);
        StoryEntity[] All();
        void Remove(string title);
        void Clear();
    }

    public class StoryRepostiory : IStoryRepository
    {
        private readonly List<StoryEntity> stories = new List<StoryEntity>();
        private ConcurrentDictionary<int, string> StoryTitleReference { get; } = new ConcurrentDictionary<int, string>();

        public void Add(string title, string content)
        {
            stories.Add(new StoryEntity(title, content));
            StoryTitleReference.TryAdd(title.GetHashCode(), title);
        }

        public StoryEntity Get(string title)
        {
            return stories.FirstOrDefault(x => x.Title == title);
        }

        public StoryEntity[] All()
        {
            return stories.ToArray();
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
