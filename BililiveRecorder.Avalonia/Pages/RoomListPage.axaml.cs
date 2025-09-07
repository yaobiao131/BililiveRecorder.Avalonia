using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using BililiveRecorder.Avalonia.Controls;
using BililiveRecorder.Common;
using BililiveRecorder.Core;
using FluentAvalonia.UI.Controls;
using Serilog;
using SelectTemplateEventArgs = Avalonia.Controls.SelectTemplateEventArgs;

namespace BililiveRecorder.Avalonia.Pages;

public partial class RoomListPage : UserControl
{
    private static readonly ILogger Logger = Log.ForContext<RoomListPage>();

    private readonly IRoom?[] NullRoom = [null];

    private readonly KeyIndexMappingReadOnlyList NullRoomWithMapping;

    public static readonly StyledProperty<GridLength> RoomListRowHeightProperty =
        AvaloniaProperty.Register<RoomListPage, GridLength>(nameof(RoomListRowHeight), GridLength.Star);

    public GridLength RoomListRowHeight
    {
        get => GetValue(RoomListRowHeightProperty);
        set => SetValue(RoomListRowHeightProperty, value);
    }

    public static readonly StyledProperty<GridLength> LogRowHeightProperty =
        AvaloniaProperty.Register<RoomListPage, GridLength>(nameof(LogRowHeight), new GridLength(0));

    public GridLength LogRowHeight
    {
        get => GetValue(LogRowHeightProperty);
        set => SetValue(LogRowHeightProperty, value);
    }

    static RoomListPage()
    {
        SortByProperty.Changed.AddClassHandler<RoomListPage>(OnPropertyChanged);
        RoomListProperty.Changed.AddClassHandler<RoomListPage>(OnPropertyChanged);
    }

    public RoomListPage()
    {
        NullRoomWithMapping = new KeyIndexMappingReadOnlyList(NullRoom);

        SortBy = SortedBy.None;
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == DataContextProperty)
        {
            if (e.OldValue is IRecorder dataOld)
                ((INotifyCollectionChanged)dataOld.Rooms).CollectionChanged -= DataSource_CollectionChanged;
            if (e.NewValue is IRecorder dataNew)
                ((INotifyCollectionChanged)dataNew.Rooms).CollectionChanged += DataSource_CollectionChanged;
            ApplySort();
        }
    }

    public static readonly StyledProperty<object> RoomListProperty =
        AvaloniaProperty.Register<RoomListPage, object>(nameof(RoomList));

    public object RoomList
    {
        get => GetValue(RoomListProperty);
        set => SetValue(RoomListProperty, value);
    }

    public static readonly StyledProperty<SortedBy> SortByProperty =
        AvaloniaProperty.Register<RoomListPage, SortedBy>(nameof(SortBy), SortedBy.None);

    public SortedBy SortBy
    {
        get => GetValue(SortByProperty);
        set
        {
            SetValue(SortByProperty, value);
            ApplySort();
        }
    }

    private static void OnPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e) =>
        ((RoomListPage)d).PrivateOnPropertyChanged(e);

    private void PrivateOnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    private void DataSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ApplySort();

    private void ApplySort()
    {
        try
        {
            if (DataContext is not IRecorder recorder || recorder.Rooms.Count == 0)
            {
                RoomList = NullRoomWithMapping;
            }
            else
            {
                var data = recorder.Rooms;
                IEnumerable<IRoom> orderedData = SortBy switch
                {
                    SortedBy.RoomId => data.OrderBy(x => x.ShortId == 0 ? x.RoomConfig.RoomId : x.ShortId),
                    SortedBy.Status => from x in data
                        orderby x.Recording descending, x.RoomConfig.AutoRecord descending, x.Streaming descending
                        select x,
                    _ => data,
                };
                var result = new KeyIndexMappingReadOnlyList(orderedData.Concat(NullRoom).ToArray());
                Dispatcher.UIThread.Invoke(() => { RoomList = result; }, DispatcherPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error Sorting");
        }
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void RoomCard_DeleteRequested(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        if (DataContext is IRecorder rec && sender is IRoom room)
        {
            try
            {
                var dialog = new DeleteRoomConfirmDialog
                {
                    DataContext = room
                };

                var result = await dialog.ShowAndDisableMinimizeToTrayAsync();

                if (result == ContentDialogResult.Primary)
                {
                    rec.RemoveRoom(room);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void RoomCard_ShowSettingsRequested(object sender, EventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        try
        {
            await new PerRoomSettingsDialog
            {
                DataContext = sender
            }.ShowAndDisableMinimizeToTrayAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }
#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void AddRoomCard_AddRoomRequested(object sender, string? e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        var input = e?.Trim();
        if (string.IsNullOrWhiteSpace(input) || DataContext is not IRecorder rec) return;
        var platform = Platform.Unknown;
        if (!long.TryParse(input, out var roomid))
        {
            var m = RoomIdFromUrl.BiliBiliRegex().Match(input);
            if (m is { Success: true, Groups.Count: > 1 } && long.TryParse(m.Groups[1].Value, out var result2))
            {
                roomid = result2;
                platform = Platform.BiliBili;
                goto checkRoomId;
            }

            m = RoomIdFromUrl.DouyinRegex().Match(input);
            if (m is { Success: true, Groups.Count: > 1 } && long.TryParse(m.Groups[1].Value, out result2))
            {
                roomid = result2;
                platform = Platform.Douyin;
                goto checkRoomId;
            }

            m = RoomIdFromUrl.DouyuRegex().Match(input);
            if (m is { Success: true, Groups.Count: > 1 } && long.TryParse(m.Groups[1].Value, out result2))
            {
                roomid = result2;
                platform = Platform.Douyu;
                goto checkRoomId;
            }

            m = RoomIdFromUrl.HuyaRegex().Match(input);
            if (m is { Success: true, Groups.Count: > 1 } && long.TryParse(m.Groups[1].Value, out result2))
            {
                roomid = result2;
                platform = Platform.Huya;
                goto checkRoomId;
            }
            else
            {
                try
                {
                    await new AddRoomFailedDialog
                    {
                        DataContext = AddRoomFailedDialog.AddRoomFailedErrorText.InvalidInput
                    }.ShowAsync();
                }
                catch (Exception)
                {
                    // ignored
                }

                return;
            }
        }

        checkRoomId:
        if (roomid < 0)
        {
            try
            {
                await new AddRoomFailedDialog
                {
                    DataContext = AddRoomFailedDialog.AddRoomFailedErrorText.RoomIdNegative
                }.ShowAsync();
            }
            catch (Exception)
            {
                // ignored
            }

            return;
        }

        if (roomid == 0)
        {
            try
            {
                await new AddRoomFailedDialog
                {
                    DataContext = AddRoomFailedDialog.AddRoomFailedErrorText.RoomIdZero
                }.ShowAsync();
            }
            catch (Exception)
            {
                // ignored
            }

            return;
        }

        if (rec.Rooms.Any(x => x.RoomConfig.RoomId == roomid || x.ShortId == roomid))
        {
            try
            {
                await new AddRoomFailedDialog
                {
                    DataContext = AddRoomFailedDialog.AddRoomFailedErrorText.Duplicate
                }.ShowAsync();
            }
            catch (Exception)
            {
                // ignored
            }

            return;
        }

        rec.AddRoom(roomid, platform);
    }

    private void MenuItem_EnableAutoRecAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not IRecorder rec) return;

        foreach (var room in rec.Rooms)
            room.RoomConfig.AutoRecord = true;

        rec.SaveConfig();
    }

    private void MenuItem_DisableAutoRecAll_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not IRecorder rec) return;

        foreach (var room in rec.Rooms)
            room.RoomConfig.AutoRecord = false;

        rec.SaveConfig();
    }

    private void MenuItem_SortBy_Click(object sender, RoutedEventArgs e) =>
        SortBy = (SortedBy)((MenuItem)sender).Tag;

    private void MenuItem_ShowLog_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        if (menuItem.IsChecked)
        {
            Splitter.IsVisible = true;
            LogElement.IsVisible = true;
            RoomListRowHeight = new GridLength(1, GridUnitType.Star);
            LogRowHeight = new GridLength(1, GridUnitType.Star);
        }
        else
        {
            Splitter.IsVisible = false;
            LogElement.IsVisible = false;
            RoomListRowHeight = new GridLength(1, GridUnitType.Star);
            LogRowHeight = new GridLength(0);
        }
    }

    private async void MenuItem_OpenWorkDirectory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is IRecorder rec)
            {
                var workDirectory = rec.Config.Global.WorkDirectory;
                var launcher = App.TopLevel.Launcher;
                if (workDirectory == null) return;
                var workDirStorage =
                    await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(workDirectory));
                if (workDirStorage == null) return;
                await launcher.LaunchFileAsync(workDirStorage);
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void MenuItem_SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is IRecorder rec)
                rec.SaveConfig();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void MenuItem_ChangeWorkPath_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Debug("ChangeWorkPath menu button invoked");
            Process.Start(typeof(RoomListPage).Assembly.Location, "run --ask-path");
            App.MainWindow?.CloseWithoutConfirmAction();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async void MenuItem_ShowLogFilesInExplorer_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var location = Path.GetDirectoryName(typeof(RoomListPage).Assembly.Location);
            if (location == null) return;
            var logPath = Path.Combine(location, "logs");
            var logStorageItem = await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(logPath));
            if (logStorageItem == null) return;
            await App.TopLevel.Launcher.LaunchFileAsync(logStorageItem);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void MenuItem_RefreshAllRoomInfo_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is IRecorder rec)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(200);

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    if (await MessageBox.Show(
                            "录播姬会自动检测直播间状态，不需要手动刷新。\n录播姬主要通过接收mikufans服务器的推送来更新状态，直播服务器会给录播姬实时发送开播通知，延迟极低。\n\n" +
                            "频繁刷新直播间状态、短时间内大量请求mikufans直播 API 可能会导致你的 IP 被屏蔽，完全无法录播。\n\n本功能是特殊情况下确实需要刷新所有直播间信息时使用的。\n\n是否要刷新所有直播间的信息？\n（每个直播间会发送一个请求）",
                            "mikufans录播姬", MessageBoxButton.YesNo) != DialogResult.Yes) return;
                });

                foreach (var room in rec.Rooms.ToArray())
                {
                    await room.RefreshRoomInfoAsync();
                    await Task.Delay(500);
                }
            });
        }
    }

    private void RecyclingElementFactory_OnSelectTemplateKey(object? sender, SelectTemplateEventArgs e)
    {
        e.TemplateKey = e.DataContext is Room ? "NormalRoomCardTemplate" : "AddRoomCardTemplate";
    }
}

public enum SortedBy
{
    None = 0,
    RoomId,
    Status,
}

internal class KeyIndexMappingReadOnlyList : IReadOnlyList<IRoom?>, IKeyIndexMapping
{
    private readonly IReadOnlyList<IRoom?> data;

    public KeyIndexMappingReadOnlyList(IReadOnlyList<IRoom?> data)
    {
        this.data = data;
    }

    public IRoom? this[int index] => data[index];

    public int Count => data.Count;

    public IEnumerator<IRoom?> GetEnumerator() => data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)data).GetEnumerator();

    #region IKeyIndexMapping

    private int lastRequestedIndex = IndexNotFound;
    private const int IndexNotFound = -1;

    // When UniqueIDs are supported, the ItemsRepeater caches the unique ID for each item
    // with the matching UIElement that represents the item.  When a reset occurs the
    // ItemsRepeater pairs up the already generated UIElements with items in the data
    // source.
    // ItemsRepeater uses IndexForUniqueId after a reset to probe the data and identify
    // the new index of an item to use as the anchor.  If that item no
    // longer exists in the data source it may try using another cached unique ID until
    // either a match is found or it determines that all the previously visible items
    // no longer exist.
    public int IndexFromKey(string uniqueId)
    {
        // We'll try to increase our odds of finding a match sooner by starting from the
        // position that we know was last requested and search forward.
        var start = lastRequestedIndex;
        for (var i = start; i < Count; i++)
        {
            if ((this[i]?.ObjectId ?? Guid.Empty).Equals(uniqueId))
                return i;
        }

        // Then try searching backward.
        start = Math.Min(Count - 1, lastRequestedIndex);
        for (var i = start; i >= 0; i--)
        {
            if ((this[i]?.ObjectId ?? Guid.Empty).Equals(uniqueId))
                return i;
        }

        return IndexNotFound;
    }

    public string KeyFromIndex(int index)
    {
        var key = this[index]?.ObjectId ?? Guid.Empty;
        lastRequestedIndex = index;
        return key.ToString();
    }

    #endregion
}
