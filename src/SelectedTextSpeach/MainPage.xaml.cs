using System;
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
        public MainPage()
        {
            InitializeComponent();
        }

        private async void TextBoxInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                await ReadContent(textBoxInput.Text);
            }
        }

        private async void TextBoxSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxInput.Text))
            {
                await ReadContent(selectedTextBox.Text);
            }
        }

        private void TextBoxInput_SelectionChanged(object sender, RoutedEventArgs e)
        {
            selectedTextBox.Text = textBoxInput.SelectedText;
            label1.Text = "Selection length is " + textBoxInput.SelectionLength.ToString();
            label2.Text = "Selection starts at " + textBoxInput.SelectionStart.ToString();
        }

        private async Task ReadContent(string content)
        {
            var mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(content);
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }
    }
}
