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
        private readonly SpeechTextBox TextBoxInput;
        private readonly SpeechTextBox TextBoxSelection;

        public MainPage()
        {
            InitializeComponent();
            if (string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                var resourceLoader = ResourceLoaderHelpers.SafeGetForCurrentViewAsync(this).Result;
                textBoxInput.Text = resourceLoader.GetString(ApplicationSettings.InitialInputTextBoxResource);
            }

            TextBoxInput = new SpeechTextBox(textBoxInput);
            TextBoxSelection = new SpeechTextBox(selectedTextBox);
        }

        private async void TextBoxInputButton_Click(object sender, RoutedEventArgs e)
        {
            await TextBoxInput.OnButtonClickAsync();
        }

        private async void TextBoxSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSelection.SetVoice(Windows.Media.SpeechSynthesis.VoiceGender.Female);
            await TextBoxSelection.OnButtonClickAsync();
        }

        private void TextBoxInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            selectedTextBox.Text = textBoxInput.SelectedText;
            label1.Text = "Selection length is " + textBoxInput.SelectionLength.ToString();
            label2.Text = "Selection starts at " + textBoxInput.SelectionStart.ToString();
        }
    }
}
