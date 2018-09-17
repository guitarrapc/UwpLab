using System.Collections.Concurrent;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HelloWorld
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
                var resourceLoader = ResourceLoaderHelpers.SafeGetForCurrentViewAsync(this).Result;
                textBoxInput.Text = resourceLoader.GetString(ApplicationSettings.InitialInputTextBoxResource);
            }
        }

        private async void InputTextBoxReadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                await TextBoxInputReader.SetContent(textBoxInput.Text);
                TextBoxInputReader.StartReadContent();
            }
        }

        private void InputTextBoxStopButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxInputReader.StopReadContent();
        }

        private async void SelectedTextBoxReadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                await TextBoxSelectionReader.SetContent(textBoxInput.Text);
                TextBoxSelectionReader.StartReadContent();
            }
            TextBoxSelectionReader.SetVoice(Windows.Media.SpeechSynthesis.VoiceGender.Female);
        }

        private void SelectedTextBoxStopButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSelectionReader.StopReadContent();
        }

        private void TextBoxInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            selectedTextBox.Text = textBoxInput.SelectedText;
            label1.Text = "Selection length is " + textBoxInput.SelectionLength.ToString();
            label2.Text = "Selection starts at " + textBoxInput.SelectionStart.ToString();
        }
    }
}
