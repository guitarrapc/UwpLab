using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Reactive.Bindings;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SelectedTextSpeach.Models
{
    public enum SpeechLanugage
    {
        en,
        ja,
    }
    public interface IContentReader
    {
        Action<object, RoutedEventArgs> SeekCompletedAction { get; set; }
        bool IsPlaying { get; }
        bool IsPaused { get; }
        bool IsStopped { get; }
        ReactiveProperty<string> PlayIcon { get; }

        void SetLanguage(SpeechLanugage language);
        void SetVoice(VoiceGender gender);
        Task SetContent(string content);
        void StartReadContent();
        void StopReadContent();
        void PauseReadContent();
    }


    public class ContentReaderModel : IContentReader, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //private static readonly string playIcon = "\xE768";
        //private static readonly string pauseIcon = "\xE769";
        private static readonly string playIcon = "123";
        private static readonly string pauseIcon = "098";

        public Action<object, RoutedEventArgs> SeekCompletedAction { get; set; }
        public bool IsPlaying => MediaElementItem.CurrentState == MediaElementState.Playing;
        public bool IsPaused => MediaElementItem.CurrentState == MediaElementState.Paused;
        public bool IsStopped => MediaElementItem.CurrentState == MediaElementState.Stopped;
        public ReactiveProperty<string> PlayIcon { get; } = new ReactiveProperty<string>(playIcon);

        private readonly MediaElement MediaElementItem = new MediaElement();
        private VoiceInformation voice = null;
        private string language = "en-US";

        public void SetLanguage(SpeechLanugage language)
        {
            switch (language)
            {
                case SpeechLanugage.en:
                    this.language = "en-US";
                    break;
                case SpeechLanugage.ja:
                    this.language = "ja-JP";
                    break;
                default:
                    this.language = "en-US";
                    break;
            }
        }
        public void SetVoice(VoiceGender gender)
        {
            var genders = SpeechSynthesizer.AllVoices.Where(x => x.Gender == gender);
            if (genders.Any(x => x.Language == language))
            {
                voice = genders.Where(x => x.Language == language)
                    .First();
            }
            else
            {
                voice = genders.First();
            }
        }

        public async Task SetContent(string content)
        {
            var resourceLoader = await StringsResourcesHelpers.SafeGetForCurrentViewAsync();
            var hoge = resourceLoader.GetString(ApplicationSettings.StoryTextResourceMatching.First().Value);

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
            PlayIcon.Value = pauseIcon;
        }

        public void StopReadContent()
        {
            if (MediaElementItem.CurrentState != MediaElementState.Stopped)
            {
                MediaElementItem.Stop();
                PlayIcon.Value = playIcon;
            }
        }

        public void PauseReadContent()
        {
            if (MediaElementItem.CurrentState == MediaElementState.Playing && MediaElementItem.CanPause)
            {
                MediaElementItem.Pause();
                PlayIcon.Value = playIcon;
            }
        }
    }
}
