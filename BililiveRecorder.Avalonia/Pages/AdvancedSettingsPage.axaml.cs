using Avalonia.Controls;
using Avalonia.Interactivity;
using BililiveRecorder.Avalonia.Controls;
using BililiveRecorder.Common.Api;
using BililiveRecorder.Common.Scripting;
using Serilog;

namespace BililiveRecorder.Avalonia.Pages;

/// <summary>
/// Interaction logic for AdvancedSettingsPage.xaml
/// </summary>
public partial class AdvancedSettingsPage : UserControl
{
    private static readonly ILogger Logger = Log.ForContext<AdvancedSettingsPage>();
    private readonly ICookieTester? httpApiClient;
    private readonly UserScriptRunner? userScriptRunner;

    public AdvancedSettingsPage(ICookieTester? httpApiClient, UserScriptRunner? userScriptRunner)
    {
        InitializeComponent();
        this.httpApiClient = httpApiClient;
        this.userScriptRunner = userScriptRunner;
    }

    public AdvancedSettingsPage() : this(
        (ICookieTester?)RootPage.ServiceProvider?.GetService(typeof(ICookieTester)),
        (UserScriptRunner?)RootPage.ServiceProvider?.GetService(typeof(UserScriptRunner))
    )
    {
    }

    private void Crash_Click(object sender, RoutedEventArgs e) => throw new TestException("test crash triggered");

    public class TestException : Exception
    {
        public TestException()
        {
        }

        public TestException(string message) : base(message)
        {
        }

        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    private void Throw_In_Task_Click(object sender, RoutedEventArgs e) =>
        _ = Task.Run(() => throw new TestException("test task exception triggered"));

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void TestCookie_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        try
        {
            await TestCookieAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Exception in TestCookie");
            await MessageBox.Show(ex.ToString(), "Cookie Test - Error", MessageBoxButton.OK);
        }
    }

    private async Task TestCookieAsync()
    {
        bool succeed;
        string message;

        if (httpApiClient is null)
            (succeed, message) = (false, "No Http Client Available");
        else
            (succeed, message) = await httpApiClient.TestCookieAsync().ConfigureAwait(false);

        if (succeed)
            await MessageBox.Show(message, "Cookie Test - Succeed", MessageBoxButton.OK);
        else
            await MessageBox.Show(message, "Cookie Test - Failed", MessageBoxButton.OK);
    }

    private void TestScript_Click(object? sender, RoutedEventArgs routedEventArgs)
    {
        _ = Task.Run(() => userScriptRunner?.CallOnTest(Log.Logger, str => MessageBox.Show(str)));
    }
}
