using Avalonia.Controls;
using Avalonia.Interactivity;
using BililiveRecorder.Avalonia.Pages;
using BililiveRecorder.Common;

namespace BililiveRecorder.Avalonia.Controls;

public partial class RoomCard : UserControl
{
    public RoomCard()
    {
        InitializeComponent();
    }

    public event EventHandler? DeleteRequested;

    public event EventHandler? ShowSettingsRequested;

    private void MenuItem_StartRecording_Click(object sender, RoutedEventArgs e) =>
        (DataContext as IRoom)?.StartRecord();

    private void MenuItem_StopRecording_Click(object sender, RoutedEventArgs e) => (DataContext as IRoom)?.StopRecord();

#pragma warning disable VSTHRD110 // Observe result of async calls
    private void MenuItem_RefreshInfo_Click(object sender, RoutedEventArgs e) =>
        (DataContext as IRoom)?.RefreshRoomInfoAsync();
#pragma warning restore VSTHRD110 // Observe result of async calls

    private void MenuItem_StartMonitor_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is IRoom room) room.RoomConfig.AutoRecord = true;
    }

    private void MenuItem_StopMonitor_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is IRoom room) room.RoomConfig.AutoRecord = false;
    }

    private void MenuItem_DeleteRoom_Click(object sender, RoutedEventArgs e) =>
        DeleteRequested?.Invoke(DataContext, EventArgs.Empty);

    private void MenuItem_ShowSettings_Click(object sender, RoutedEventArgs e) =>
        ShowSettingsRequested?.Invoke(DataContext, EventArgs.Empty);

    private void Button_Split_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is IRoom room) room.SplitOutput();
    }

#pragma warning disable VSTHRD100
    private async void MenuItem_OpenInBrowser_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (DataContext is IRoom r)
        {
            var url = r.RoomConfig.Platform switch
            {
                Platform.Huya => "https://www.huya.com/" + r.RoomConfig.RoomId,
                Platform.BiliBili => "https://live.bilibili.com/" + r.RoomConfig.RoomId,
                Platform.Douyin => "https://live.douyin.com/" + r.RoomConfig.RoomId,
                Platform.Douyu => "https://douyu.com/" + r.RoomConfig.RoomId,
                _ => ""
            };
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                await App.TopLevel.Launcher.LaunchUriAsync(uri);
            }
        }
    }

    private void MenuItem_ShowGlobalSettings_Click(object sender, RoutedEventArgs e)
    {
        if (RootPage.SwitchToSettingsPage is { } change)
        {
            change();
        }
    }
}
