using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Controls;

internal static class ContentDialogExtensions
{
    internal static async Task<ContentDialogResult> ShowAndDisableMinimizeToTrayAsync(this ContentDialog contentDialog)
    {
        var mw = App.MainWindow;
        mw.HideToTrayBlockedByContentDialog = true;

#pragma warning disable RS0030 // Do not used banned APIs
        var result = await contentDialog.ShowAsync();
#pragma warning restore RS0030 // Do not used banned APIs

        mw.HideToTrayBlockedByContentDialog = false;

        return result;
    }
}
