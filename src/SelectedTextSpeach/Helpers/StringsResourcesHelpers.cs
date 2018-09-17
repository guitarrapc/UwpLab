using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace SelectedTextSpeach
{
    // About String Resources : https://docs.microsoft.com/ja-jp/windows/uwp/app-resources/localize-strings-ui-manifest
    public static class StringsResourcesHelpers
    {
        public static async Task<ResourceLoader> SafeGetForCurrentViewAsync(Page page)
        {
            ResourceLoader loader = null;
            if (CoreWindow.GetForCurrentThread() != null)
            {
                loader = ResourceLoader.GetForCurrentView();
            }
            else
            {
                await page.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    loader = ResourceLoader.GetForCurrentView();
                });
            }
            return loader;
        }
    }
}
