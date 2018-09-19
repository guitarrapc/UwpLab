using System;
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
    internal class MainPageViewModel : IDisposable
    {
        public ReactiveProperty<string> Input { get; }
        public ReadOnlyReactiveProperty<string> Output { get; }
        public Person SelectedItem { get; set; }
        public ReactiveProperty<string> InputTextBox { get; }
        public ReactiveProperty<string> PlayIcon { get; } = new ReactiveProperty<string>();
        public Story[] ListViewItemStory { get; }

        private readonly StoryModel storyModel = new StoryModel();
        private readonly ContentReaderModel contentReaderModel = new ContentReaderModel();
        private readonly ContentReaderModel TextBoxInputReader = new ContentReaderModel();
        private CompositeDisposable disposable = new CompositeDisposable();

        public MainPageViewModel()
        {
            Input = new ReactiveProperty<string>("");
            Output = Input
                .Delay(TimeSpan.FromSeconds(1))
                .Select(x => x.ToUpper())
                .ToReadOnlyReactiveProperty();

            InputTextBox = new ReactiveProperty<string>(storyModel.FirstOrDefailt()?.Content);
            contentReaderModel.PlayIcon.Select(x => x.ToString()).Subscribe(x =>
            {
                PlayIcon.Value = x;
                System.Diagnostics.Debug.WriteLine($"model : {x}");
            }).AddTo(disposable);
            PlayIcon.Subscribe(x => System.Diagnostics.Debug.WriteLine(x)).AddTo(disposable);
            ListViewItemStory = storyModel.All();
        }

        public async void Dump()
        {
            var message = $"{SelectedItem.Name} selected.";
            var dlg = new MessageDialog(message);
            await dlg.ShowAsync();
        }

        public Story GetStory(int titleHash)
        {
            return storyModel.Get(titleHash);
        }

        public async Task ReadInputBox()
        {
            if (TextBoxInputReader.IsPlaying)
            {
                TextBoxInputReader.PauseReadContent();
            }
            else if (TextBoxInputReader.IsPaused)
            {
                TextBoxInputReader.StartReadContent();
            }
            else if (!string.IsNullOrWhiteSpace(InputTextBox.Value))
            {
                await TextBoxInputReader.SetContent(InputTextBox.Value);
                TextBoxInputReader.StartReadContent();
            }
        }

        public void StopInputBox()
        {
            TextBoxInputReader.StopReadContent();
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
