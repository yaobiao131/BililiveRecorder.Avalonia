using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using BililiveRecorder.Avalonia.Controls;
using BililiveRecorder.Avalonia.Pages;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using Serilog;

namespace BililiveRecorder.Avalonia;

public partial class NewMainWindow : AppWindow
{
    public string SoftwareVersion { get; }

    public NewMainWindow()
    {
        SoftwareVersion = GitVersionInformation.FullSemVer;
        InitializeComponent();
        SingleInstance.NotificationReceived += SingleInstance_NotificationReceived;
        // this.GetObservable(WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(state => { }));
    }

    private void SingleInstance_NotificationReceived(object? sender, EventArgs e) => SuperActivateAction();

    public event EventHandler? NativeBeforeWindowClose;

    // internal Action<string, string, BalloonIcon>? ShowBalloonTipCallback { get; set; }

    internal void CloseWithoutConfirmAction()
    {
        CloseConfirmed = true;
        Dispatcher.UIThread.Invoke(Close, DispatcherPriority.Normal);
    }

    internal void SuperActivateAction()
    {
        try
        {
            Show();
            WindowState = WindowState.Normal;
            Topmost = true;
            Activate();
            Topmost = false;
            Focus();
        }
        catch (Exception)
        {
        }
    }

    private bool notification_showed;
    public bool HideToTray { get; set; }
    public bool HideToTrayBlockedByContentDialog { get; set; }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (HideToTray && !HideToTrayBlockedByContentDialog && WindowState == WindowState.Minimized)
        {
            Hide();
            if (!notification_showed)
            {
                notification_showed = true;
                // var title = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:TaskbarIconControl_Title");
                // var body = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:TaskbarIconControl_MinimizedNotification");
                // this.ShowBalloonTipCallback?.Invoke(title, body, BalloonIcon.Info);
            }
        }
    }

    #region Confirm Close Window

    private bool CloseConfirmed;

    private readonly SemaphoreSlim CloseWindowSemaphoreSlim = new(1, 1);

    public bool PromptCloseConfirm { get; set; } = true;

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void Window_Closing(object? sender, WindowClosingEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        if (PromptCloseConfirm && !CloseConfirmed)
        {
            e.Cancel = true;

            if (await CloseWindowSemaphoreSlim.WaitAsync(0))
            {
                try
                {
                    if (await new CloseWindowConfirmDialog().ShowAndDisableMinimizeToTrayAsync() == ContentDialogResult.Primary)
                    {
                        CloseConfirmed = true;
                        Dispatcher.UIThread.Invoke(Close, DispatcherPriority.Normal);
                        return;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    CloseWindowSemaphoreSlim.Release();
                }
            }
        }
        else
        {
            SingleInstance.NotificationReceived -= SingleInstance_NotificationReceived;
            Log.Logger.ForContext<NewMainWindow>().Debug("Window Closing");
            NativeBeforeWindowClose?.Invoke(this, EventArgs.Empty);
            return;
        }
    }

    #endregion

    private void Window_OnLoaded(object? sender, RoutedEventArgs e)
    {
        Content = new RootPage();
    }
}
