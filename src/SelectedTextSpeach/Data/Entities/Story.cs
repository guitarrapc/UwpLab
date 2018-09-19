namespace SelectedTextSpeach.Data.Entities
{
    public class Story
    {
        public string Title { get; private set; }
        public string Content { get; private set; }

        private int TitleHash;

        public Story(string title, string content)
        {
            Title = title;
            Content = content;
            TitleHash = Title.GetHashCode();
        }
    }
}
