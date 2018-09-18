using System.Collections.Concurrent;
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
        private readonly IContentReader TextBoxInputReader = new ContentReader();
        private readonly IContentReader TextBoxSelectionReader = new ContentReader();

        public MainPage()
        {
            InitializeComponent();
            if (string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                var resourceLoader = StringsResourcesHelpers.SafeGetForCurrentViewAsync(this).Result;
                textBoxInput.Text = resourceLoader.GetString(ApplicationSettings.InitialInputTextBoxResource);
            }
        }

        private async void InputTextBoxPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxInputReader.IsPlaying)
            {
                TextBoxInputReader.PauseReadContent();
            }
            else if (TextBoxInputReader.IsPaused)
            {
                TextBoxInputReader.StartReadContent();
            }
            else if (!string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                await TextBoxInputReader.SetContent(textBoxInput.Text);
                TextBoxInputReader.StartReadContent();
            }
            inputTextReadButton.Content = TextBoxInputReader.CurrentIconContent;
        }
        private void InputTextBoxStopButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxInputReader.StopReadContent();
            inputTextReadButton.Content = TextBoxInputReader.CurrentIconContent;
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
            selectedTextReadButton.Content = TextBoxSelectionReader.CurrentIconContent;
        }
        private void SelectedTextBoxStopButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSelectionReader.StopReadContent();
        }

        private void TextBoxInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            textBoxSelected.Text = textBoxInput.SelectedText;
            label1.Text = "Selection length is " + textBoxInput.SelectionLength.ToString();
            label2.Text = "Selection starts at " + textBoxInput.SelectionStart.ToString();
        }
    }
}
