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
        public PersonEntity SelectedItem { get; set; }

        public ReactiveProperty<string> TextBoxInput { get; }
        public ReactiveProperty<string> PlayIconTextBoxInput { get; }
        //public ReadOnlyReactiveCollection<string> SpeechTitles { get; }
        public ReadOnlyReactiveCollection<StoryEntity> SpeechTitles { get; }

        public ReactiveProperty<string> TextBoxSelection { get; }
        public ReactiveProperty<string> PlayIconTextBoxSelection { get; }

        private readonly IStoryModel storyModel = new HarryPotterStoryModel();
        private readonly IContentReader TextBoxInputReader = new ContentReaderModel();
        private readonly IContentReader TextBoxSelectionReader = new ContentReaderModel();
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

            SpeechTitles = storyModel.AllStories.ToReadOnlyReactiveCollection();

            TextBoxInput = new ReactiveProperty<string>(storyModel.InitialStory.Content);
            TextBoxSelection = new ReactiveProperty<string>("");

            PlayIconTextBoxInput = new ReactiveProperty<string>(playIcon);
            PlayIconTextBoxSelection = new ReactiveProperty<string>(playIcon);
            storyModel.CurrentStory.Subscribe(x => TextBoxInput.Value = x.Content).AddTo(disposable);
        }

        public async void Dump()
        {
            var message = $"{SelectedItem.Name} selected.";
            var dlg = new MessageDialog(message);
            await dlg.ShowAsync();
        }

        public void StorySelectionChanged(string title)
        {
            storyModel.ChangeCurrentStoryByTitle(title);
        }

        public async Task ReadInputBox()
        {
            if (TextBoxInputReader.IsPlaying)
            {
                TextBoxInputReader.PauseReadContent();
                PlayIconTextBoxInput.Value = playIcon;
            }
            else if (TextBoxInputReader.IsPaused)
            {
                TextBoxInputReader.StartReadContent();
                PlayIconTextBoxInput.Value = pauseIcon;
            }
            else if (!string.IsNullOrWhiteSpace(TextBoxInput.Value))
            {
                await TextBoxInputReader.SetContent(TextBoxInput.Value);
                TextBoxInputReader.StartReadContent();
                PlayIconTextBoxInput.Value = pauseIcon;
            }
        }

        public void StopInputBox()
        {
            TextBoxInputReader.StopReadContent();
            PlayIconTextBoxInput.Value = playIcon;
        }

        public async Task ReadSelectionBox()
        {
            if (TextBoxSelectionReader.IsPlaying)
            {
                TextBoxSelectionReader.PauseReadContent();
                PlayIconTextBoxSelection.Value = pauseIcon;
            }
            else if (TextBoxSelectionReader.IsPaused)
            {
                TextBoxSelectionReader.StartReadContent();
                PlayIconTextBoxSelection.Value = playIcon;
            }
            else if (!string.IsNullOrWhiteSpace(TextBoxSelection.Value))
            {
                await TextBoxSelectionReader.SetContent(TextBoxSelection.Value);
                TextBoxSelectionReader.StartReadContent();
                PlayIconTextBoxSelection.Value = pauseIcon;
            }
        }

        public void StopSelectionBox()
        {
            TextBoxSelectionReader.StopReadContent();
            PlayIconTextBoxSelection.Value = playIcon;
        }


        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
