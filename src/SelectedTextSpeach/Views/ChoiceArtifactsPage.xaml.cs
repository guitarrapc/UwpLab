using SelectedTextSpeach.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace SelectedTextSpeach.Views
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class ChoiceArtifactsPage : Page
    {
        private ChoiceArtifactViewModel ViewModel { get; } = new ChoiceArtifactViewModel();

        public ChoiceArtifactsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // このようにe.Parameterで前のページから渡された値を取得できます。
            // 値はキャストして取り出します。
            var param = e.Parameter as string;

            base.OnNavigatedTo(e);
        }

        public async void NavigateMainPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage), "fugafuga");
        }

    }
}
