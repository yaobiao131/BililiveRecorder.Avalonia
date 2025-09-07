using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace BililiveRecorder.Avalonia;

public class App : Application
{
    public static NewMainWindow MainWindow { get; private set; } = null!;
    public static TopLevel TopLevel { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Lang.Resources.Culture = CultureInfo.CurrentCulture;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new NewMainWindow();
            desktop.MainWindow = MainWindow;
            TopLevel = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
