using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
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

        public MainPage()
        {
            InitializeComponent();
            if (string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                var resourceLoader = ResourceLoaderHelpers.SafeGetForCurrentViewAsync(this).Result;
                textBoxInput.Text = resourceLoader.GetString(ApplicationSettings.InitialInputTextBoxResource);
            }
        }

        private async void TextBoxInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                if (!playerDictionary.TryGetValue(nameof(TextBoxInputButton_Click), out var mediaElement))
                {
                    mediaElement = new MediaElement();
                    playerDictionary.TryAdd(nameof(TextBoxInputButton_Click), mediaElement);
                }
                await ReadContent(mediaElement, textBoxInput.Text);
            }
        }

        private async void TextBoxSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                if (!playerDictionary.TryGetValue(nameof(TextBoxSelectedButton_Click), out var mediaElement))
                {
                    mediaElement = new MediaElement();
                    playerDictionary.TryAdd(nameof(TextBoxSelectedButton_Click), mediaElement);
                }
                await ReadContent(mediaElement, selectedTextBox.Text);
            }
        }

        private void TextBoxInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            selectedTextBox.Text = textBoxInput.SelectedText;
            label1.Text = "Selection length is " + textBoxInput.SelectionLength.ToString();
            label2.Text = "Selection starts at " + textBoxInput.SelectionStart.ToString();
        }

        private async Task ReadContent(MediaElement mediaElement, string content)
        {
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(content);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.SeekCompleted += (obj, player) =>
            {
                // Change Media Element Status to completed
            };
            mediaElement.Play();
        }

        private async Task StopReadContent(string content)
        {
            var mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(content);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }
    }
}
