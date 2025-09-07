using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using BililiveRecorder.Avalonia.Controls;
using BililiveRecorder.Avalonia.Models;
using BililiveRecorder.Common.Config;
using BililiveRecorder.Common.Config.V3;
using BililiveRecorder.Core;
using BililiveRecorder.DependencyInjection;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BililiveRecorder.Avalonia.Pages;

public partial class RootPage : UserControl
{
    private static readonly ILogger Logger = Log.ForContext<RootPage>();

    internal static string? CommandArgumentRecorderPath = null;
    internal static bool CommandArgumentFirstRun = false; // TODO
    internal static bool CommandArgumentAskPath = false;
    internal static bool CommandArgumentHide = false;

    private readonly Dictionary<string, Type> PageMap = new();
    private readonly WorkDirectoryLoader workDirectoryLoader = new();
    private readonly NavigationTransitionInfo transitionInfo = new DrillInNavigationTransitionInfo();

    private int SettingsClickCount;

    internal static IServiceProvider? ServiceProvider { get; private set; }
    private ServiceProvider serviceProvider = null!;
    internal RootModel Model { get; private set; }

    internal static Action? SwitchToSettingsPage { get; private set; }

    public RootPage()
    {
        void AddType(Type t) => PageMap.Add(t.Name, t);
        AddType(typeof(RoomListPage));
        AddType(typeof(SettingsPage));
        AddType(typeof(LogPage));
        AddType(typeof(AboutPage));
        AddType(typeof(AdvancedSettingsPage));
        AddType(typeof(AnnouncementPage));
        AddType(typeof(ToolboxAutoFixPage));
        AddType(typeof(ToolboxRemuxPage));
        AddType(typeof(ToolboxDanmakuMergerPage));

        Model = new RootModel();
        DataContext = Model;
        InitializeComponent();
        AdvancedSettingsPageItem.Opacity = 0;

        try
        {
            _ = new System.Globalization.CultureInfo("en-PN");
        }
        catch (Exception)
        {
            JokeLangSelectionMenuItem.IsVisible = false;
        }
#if DEBUG
        DebugBuildIcon.IsVisible = true;
#endif
        var mw = App.MainWindow;
        mw.NativeBeforeWindowClose += RootPage_NativeBeforeWindowClose;
        Loaded += RootPage_Loaded;
        SwitchToSettingsPage = () => { SettingsPageNavigationViewItem.IsSelected = true; };
    }

    private void RootPage_NativeBeforeWindowClose(object? sender, EventArgs e)
    {
        Model.Dispose();
        SingleInstance.Cleanup();
    }

    private async void RootPage_Loaded(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;
        if (CommandArgumentFirstRun)
        {
        }

        // 上次选择的路径信息
        var pathInfo = workDirectoryLoader.Read();
        // 第一次尝试从命令行和配置文件自动选择路径
        var first_time = true;
        // 如果是从命令行参数传入的路径，则不保存选择的路径到文件
        var from_argument = false;

        // 路径选择错误
        var error = WorkDirectorySelectorDialog.WorkDirectorySelectorDialogError.None;
        // 最终选择的路径
        string path;
        while (true)
        {
            try
            {
                // 获取一个路径
                if (first_time)
                {
                    // while 循环第一次运行时检查命令行参数、和上次选择记住的路径
                    try
                    {
                        first_time = false;

                        if (!string.IsNullOrWhiteSpace(CommandArgumentRecorderPath))
                        {
                            // 如果有参数直接跳到检查路径
                            Logger.Debug("Using path from command argument");
                            path = Path.GetFullPath(CommandArgumentRecorderPath);
                            from_argument = true; // 用于控制不写入文件保存
                        }
                        else if (pathInfo.SkipAsking && !CommandArgumentAskPath)
                        {
                            Logger.Debug("Using path from path.json file");
                            // 上次选择了“不再询问”
                            path = pathInfo.Path;
                        }
                        else
                        {
                            // 无命令行参数 和 记住选择
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        // 出错直接重新来，不显示 error
                        continue;
                    }
                }
                else
                {
                    // 尝试读取上次选择的路径
                    var lastdir = pathInfo.Path;

                    // 显示路径选择界面
                    var dialog = new WorkDirectorySelectorDialog
                    {
                        Error = error,
                        Path = lastdir,
                        SkipAsking = pathInfo.SkipAsking,
                    };
                    var dialogResult = await dialog.ShowAsync();
                    switch (dialogResult)
                    {
                        case ContentDialogResult.Primary:
                            Logger.Debug("Confirm path selected");
                            break;
                        case ContentDialogResult.Secondary:
                            Logger.Debug("Toolbox mode selected");
                            return;
                        case ContentDialogResult.None:
                        default:
                            Logger.Debug("Exit selected");
                            App.MainWindow?.CloseWithoutConfirmAction();
                            return;
                    }

                    pathInfo.SkipAsking = dialog.SkipAsking;

                    try
                    {
                        path = Path.GetFullPath(dialog.Path);
                    }
                    catch (Exception)
                    {
                        error = WorkDirectorySelectorDialog.WorkDirectorySelectorDialogError.PathNotSupported;
                        continue;
                    }
                }
                // 获取一个路径结束

                var configFilePath = Path.Combine(path, "config.json");
                if (!Directory.Exists(path))
                {
                    error = WorkDirectorySelectorDialog.WorkDirectorySelectorDialogError.PathDoesNotExist;
                    continue;
                }

                if (!Directory.EnumerateFileSystemEntries(path).Any())
                {
                    // 可用的空文件夹
                }
                else if (!File.Exists(configFilePath))
                {
                    error = WorkDirectorySelectorDialog.WorkDirectorySelectorDialogError.PathContainsFiles;
                    continue;
                }
                // 已经选定工作目录

                // 如果不是从命令行参数传入的路径，写入 lastdir_path 记录
                try
                {
                    if (!from_argument)
                    {
                        pathInfo.Path = path;
                        workDirectoryLoader.Write(pathInfo);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                // 加载配置文件
                var config = ConfigParser.LoadFromDirectory(path);
                if (config is null)
                {
                    error = WorkDirectorySelectorDialog.WorkDirectorySelectorDialogError.FailedToLoadConfig;
                    continue;
                }

                config.Global.WorkDirectory = path;

                // 检查已经在同目录运行的其他进程
                if (!SingleInstance.CheckMutex(path))
                {
                    // 有已经在其他目录运行的进程，已经通知该进程，本进程退出
                    App.MainWindow.CloseWithoutConfirmAction();
                    return;
                }

                // 无已经在同目录运行的进程
                serviceProvider = BuildServiceProvider(config, Logger);
                ServiceProvider = serviceProvider;
                var recorder = serviceProvider.GetRequiredService<IRecorder>();

                Model.Recorder = recorder;
                RoomListPageNavigationViewItem.IsEnabled = true;
                SettingsPageNavigationViewItem.IsEnabled = true;
                App.MainWindow.HideToTray = true;
                NetworkChangeDetector.Enable();

                _ = Task.Run(async () =>
                {
                    await Task.Delay(150);
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        RoomListPageNavigationViewItem.IsSelected = true;

                        if (CommandArgumentHide)
                            App.MainWindow.WindowState = WindowState.Minimized;
                    }, DispatcherPriority.Normal);
                });
                break;
            }
            catch (Exception ex)
            {
                error = WorkDirectorySelectorDialog.WorkDirectorySelectorDialogError.UnknownError;
                Logger.Warning(ex, "选择工作目录时发生了未知错误");
            }
        }
    }

    private ServiceProvider BuildServiceProvider(ConfigV3 config, ILogger logger) => new ServiceCollection()
        .AddSingleton(logger)
        .AddDispatchProvider()
        .AddRecorderConfig(config)
        .AddFlv()
        .AddRecorder()
        .BuildServiceProvider();

    private void NavigationView_OnSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs args)
    {
        SettingsClickCount = 0;
        if (args.IsSettingsSelected)
        {
            MainFrame.Navigate(typeof(SettingsPage), null, transitionInfo);
        }
        else
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            var selectedItemTag = selectedItem.Tag?.ToString() ?? "";
            if (PageMap.TryGetValue(selectedItemTag, out var pageType))
            {
                MainFrame.Navigate(pageType, null, transitionInfo);
            }
        }
    }

    private void SettingsPageNavigationViewItem_OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.PointerUpdateKind != PointerUpdateKind.RightButtonReleased)
        {
            return;
        }

        if (++SettingsClickCount > 1)
        {
            SettingsClickCount = 0;
            AdvancedSettingsPageItem.Opacity = Math.Abs(AdvancedSettingsPageItem.Opacity - 1) > 0 ? 1 : 0;
        }
    }

    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        try
        {
            if (sender is not Frame frame) return;

            while (frame.BackStackDepth > 0)
            {
                frame.BackStack.Clear();
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private void SwitchLightDarkTheme_Click(object sender, RoutedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
            ChangeTheme();
        else
            Dispatcher.UIThread.Invoke(ChangeTheme);

        return;

        static void ChangeTheme()
        {
            if (Application.Current == null) return;
            Application.Current.RequestedThemeVariant = Application.Current.RequestedThemeVariant == ThemeVariant.Dark
                ? ThemeVariant.Light
                : ThemeVariant.Dark;
        }
    }
}
