using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SelectedTextSpeach
{
    public interface IContentReader
    {
        Action<object, RoutedEventArgs> SeekCompletedAction { get; set; }
        bool IsPlaying { get; }
        bool IsPaused { get; }
        bool IsStopped { get; }
        string CurrentIconContent { get; }

        void SetVoice(VoiceGender gender);
        Task SetContent(string content);
        void StartReadContent();
        void StopReadContent();
        void PauseReadContent();
    }


    public class ContentReader : IContentReader
    {
        private static readonly string playIcon = "\xE768";
        private static readonly string pauseIcon = "\xE769";

        public Action<object, RoutedEventArgs> SeekCompletedAction { get; set; }
        public bool IsPlaying => MediaElementItem.CurrentState == MediaElementState.Playing;
        public bool IsPaused => MediaElementItem.CurrentState == MediaElementState.Paused;
        public bool IsStopped => MediaElementItem.CurrentState == MediaElementState.Stopped;
        public string CurrentIconContent { get; private set; } = playIcon;

        private readonly MediaElement MediaElementItem = new MediaElement();
        private VoiceInformation voice = null;

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
            if (MediaElementItem.CurrentState == MediaElementState.Paused)
            {
                MediaElementItem.Play();
            }
            else
            {
                //TODO: Auto Play's end should change button text to PlayIcon
                MediaElementItem.SeekCompleted += (obj, player) =>
                {
                    SeekCompletedAction?.Invoke(obj, player);
                };
                MediaElementItem.Play();
            }
            CurrentIconContent = pauseIcon;
        }

        public void StopReadContent()
        {
            if (MediaElementItem.CurrentState == MediaElementState.Playing)
            {
                MediaElementItem.Stop();
                CurrentIconContent = playIcon;
            }
        }

        public void PauseReadContent()
        {
            if (MediaElementItem.CurrentState == MediaElementState.Playing && MediaElementItem.CanPause)
            {
                MediaElementItem.Pause();
                CurrentIconContent = playIcon;
            }
        }
    }
}
