using System.Collections.Concurrent;
using System.Collections.Generic;
using SelectedTextSpeach.Data.Entities;
using SelectedTextSpeach.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SelectedTextSpeach.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly ConcurrentDictionary<string, MediaElement> playerDictionary = new ConcurrentDictionary<string, MediaElement>();
        private readonly static Dictionary<int, string> StoryTextReferences = new Dictionary<int, string>();

        private MainPageViewModel ViewModel { get; } = new MainPageViewModel();

        public MainPage()
        {
            InitializeComponent();
            foreach (var story in ViewModel.ListViewItemStory)
            {
                storyListView.Items.Add(new StoryTitle { Title = story.Title });
            }

            // Edit only Instance to show Initial RP Value (by DataContext)
            DataContext = ViewModel;
        }

        private void StorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.StorySelectionChanged(storyListView.SelectedItem as StoryTitle);
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.SelectedItem = listView.SelectedItem as Person;
        }

        private void TextBoxInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.TextBoxSelection.Value = textBoxInput.SelectedText;
            label1.Text = $"Selection length is {textBoxInput.SelectionLength}";
            label2.Text = $"Selection starts at {textBoxInput.SelectionStart}";
        }
    }
}
