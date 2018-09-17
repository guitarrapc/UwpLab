using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HelloWorld
{
    public interface IContentReader
    {
        void SetVoice(VoiceGender gender, string language);
        Task<(IRandomAccessStream stream, string contentType)> SetContent(string content);
        void StartReadContent(IRandomAccessStream stream, string contentType, Action<object, RoutedEventArgs> seekCompletedAction);
        void StopReadContent();
        void PauseReadContent();
        void ResumeReadContent();
    }

    public class SpeechTextBox : IContentReader
    {
        public TextBox TextBoxItem { get; set; }
        public MediaElement MediaElementItem { get; set; }

        private VoiceInformation voice = null;

        public SpeechTextBox(TextBox textBox)
        {
            TextBoxItem = textBox;
            MediaElementItem = new MediaElement();
        }

        public async Task OnButtonClickAsync(Action<object, RoutedEventArgs> seekCompletedAction = null)
        {
            if (!string.IsNullOrWhiteSpace(TextBoxItem.Text))
            {
                (IRandomAccessStream stream, string contentType) stream = await SetContent(TextBoxItem.Text);
                StartReadContent(stream.stream, stream.contentType, seekCompletedAction);
            }
        }

        public void SetVoice(VoiceGender gender, string language = "en-US")
        {
            voice = SpeechSynthesizer.AllVoices.Where(x => x.Gender == gender).First();
        }

        public async Task<(IRandomAccessStream stream, string contentType)> SetContent(string content)
        {
            using (var synth = new SpeechSynthesizer())
            {
                if (voice != null)
                {
                    synth.Voice = voice;
                }
                var stream = await synth.SynthesizeTextToStreamAsync(content);
                return (stream, stream.ContentType);
            }
        }

        public void StartReadContent(IRandomAccessStream stream, string contentType, Action<object, RoutedEventArgs> seekCompletedAction)
        {
            MediaElementItem.SetSource(stream, contentType);
            MediaElementItem.SeekCompleted += (obj, player) =>
            {
                //TODO: Change Media Element Status to completed
                seekCompletedAction?.Invoke(obj, player);
            };
            MediaElementItem.Play();
        }

        public void StopReadContent()
        {
            if (MediaElementItem.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
            {
                MediaElementItem.Stop();
            }
        }

        public void PauseReadContent()
        {
            if (MediaElementItem.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing && MediaElementItem.CanPause)
            {
                MediaElementItem.Pause();
            }
        }

        public void ResumeReadContent()
        {
            if (MediaElementItem.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Paused)
            {
                MediaElementItem.Play();
            }
        }
    }
}
