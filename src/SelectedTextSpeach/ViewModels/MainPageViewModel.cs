using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SelectedTextSpeach.Data.Entities;
using SelectedTextSpeach.Data.Models;
using SelectedTextSpeach.Models;
using Windows.UI.Popups;

namespace SelectedTextSpeach.ViewModels
{
    public class MainPageViewModel : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ReactiveProperty<string> Input { get; }
        public ReadOnlyReactiveProperty<string> Output { get; }
        public Person SelectedItem { get; set; }
        public ReactiveProperty<string> TextBoxInput { get; }
        public ReactiveProperty<string> PlayIcon { get; }
        public Story[] ListViewItemStory { get; }

        private readonly IStoryModel storyModel = new HarryPotterStoryModel();
        private readonly ContentReaderModel contentReaderModel = new ContentReaderModel();
        private readonly ContentReaderModel TextBoxInputReader = new ContentReaderModel();
        private CompositeDisposable disposable = new CompositeDisposable();

        private static readonly string playIcon = "\xE768";
        private static readonly string pauseIcon = "\xE769";

        public MainPageViewModel()
        {
            Input = new ReactiveProperty<string>("");
            Output = Input
                .Delay(TimeSpan.FromSeconds(1))
                .Select(x => x.ToUpper())
                .ToReadOnlyReactiveProperty();

            TextBoxInput = new ReactiveProperty<string>(storyModel.InitialStory.Content);
            PlayIcon = new ReactiveProperty<string>(playIcon);
            ListViewItemStory = storyModel.AllStories;

            storyModel.CurrentStory.Subscribe(x => TextBoxInput.Value = x.Content).AddTo(disposable);
        }

        public async void Dump()
        {
            var message = $"{SelectedItem.Name} selected.";
            var dlg = new MessageDialog(message);
            await dlg.ShowAsync();
        }

        public void StorySelectionChanged(int titleHash)
        {
            storyModel.ChangeCurrentStoryByTitleHash(titleHash);
        }

        public async Task ReadInputBox()
        {
            if (TextBoxInputReader.IsPlaying)
            {
                TextBoxInputReader.PauseReadContent();
                PlayIcon.Value = playIcon;
            }
            else if (TextBoxInputReader.IsPaused)
            {
                TextBoxInputReader.StartReadContent();
                PlayIcon.Value = pauseIcon;
            }
            else if (!string.IsNullOrWhiteSpace(TextBoxInput.Value))
            {
                await TextBoxInputReader.SetContent(TextBoxInput.Value);
                TextBoxInputReader.StartReadContent();
                PlayIcon.Value = pauseIcon;
            }
        }

        public void StopInputBox()
        {
            TextBoxInputReader.StopReadContent();
            PlayIcon.Value = playIcon;
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
