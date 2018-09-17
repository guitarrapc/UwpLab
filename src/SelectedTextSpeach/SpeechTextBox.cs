using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HelloWorld
{
    public interface IContentReader
    {
        Action<object, RoutedEventArgs> SeekCompletedAction { get; set; }

        void SetVoice(VoiceGender gender);
        Task SetContent(string content);
        void StartReadContent();
        void StopReadContent();
        void PauseReadContent();
        void ResumeReadContent();
    }


    public class ContentReader : IContentReader
    {
        private readonly MediaElement MediaElementItem = new MediaElement();
        private VoiceInformation voice = null;
        public Action<object, RoutedEventArgs> SeekCompletedAction { get; set; }

        public void SetVoice(VoiceGender gender)
        {
            voice = SpeechSynthesizer.AllVoices.Where(x => x.Gender == gender).First();
        }

        public async Task SetContent(string content)
        {
            using (var synth = new SpeechSynthesizer())
            {
                if (voice != null)
                {
                    synth.Voice = voice;
                }
                var stream = await synth.SynthesizeTextToStreamAsync(content);
                MediaElementItem.SetSource(stream, stream.ContentType);
            }
        }

        public void StartReadContent()
        {
            MediaElementItem.SeekCompleted += (obj, player) =>
            {
                SeekCompletedAction?.Invoke(obj, player);
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
                StartReadContent();
            }
        }
    }
}
