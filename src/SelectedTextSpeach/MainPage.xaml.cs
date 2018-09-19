using System.Collections.Concurrent;
using System.Collections.Generic;
using SelectedTextSpeach.Models;
using SelectedTextSpeach.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SelectedTextSpeach
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly ConcurrentDictionary<string, MediaElement> playerDictionary = new ConcurrentDictionary<string, MediaElement>();
        private readonly static Dictionary<int, string> StoryTextReferences = new Dictionary<int, string>();

        private MainPageViewModel ViewModel { get; } = new MainPageViewModel();
        private readonly ContentReaderModel TextBoxSelectionReader = new ContentReaderModel();

        public MainPage()
        {
            InitializeComponent();
            foreach (var story in ViewModel.ListViewItemStory)
            {
                storyListView.Items.Add(story.Title);
            }
        }

        private void StorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newStory = ViewModel.GetStory(storyListView.SelectedItem.ToString().GetHashCode());
            textBoxInput.Text = newStory.Content;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedItem = listView.SelectedItem as Person;
        }

        private async void SelectedTextBoxPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxSelectionReader.IsPlaying)
            {
                TextBoxSelectionReader.PauseReadContent();
            }
            else if (TextBoxSelectionReader.IsPaused)
            {
                TextBoxSelectionReader.StartReadContent();
            }
            else if (!string.IsNullOrWhiteSpace(textBoxSelected.Text))
            {
                await TextBoxSelectionReader.SetContent(textBoxSelected.Text);
                TextBoxSelectionReader.StartReadContent();
            }
        }
        private void SelectedTextBoxStopButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSelectionReader.StopReadContent();
        }

        private void TextBoxInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            textBoxSelected.Text = textBoxInput.SelectedText;
            label1.Text = $"Selection length is {textBoxInput.SelectionLength}";
            label2.Text = $"Selection starts at {textBoxInput.SelectionStart}";
        }
    }
}
