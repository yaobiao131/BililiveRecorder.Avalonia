using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Net;
using System.Security;
using Avalonia;
using BililiveRecorder.Flv.Pipeline;
using BililiveRecorder.Flv.Xml;
using BililiveRecorder.ToolBox;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace BililiveRecorder.Avalonia;

class Program
{
    private const int CODE__AVALONIA = 0x5F_57_50_46;

    internal static readonly LoggingLevelSwitch LevelSwitchGlobal;
    internal static readonly LoggingLevelSwitch LevelSwitchConsole;
    internal static readonly Logger Logger;

    static Program()
    {
        LevelSwitchGlobal = new LoggingLevelSwitch(LogEventLevel.Debug);
#if DEBUG
        LevelSwitchGlobal.MinimumLevel = LogEventLevel.Verbose;
#endif
        LevelSwitchConsole = new LoggingLevelSwitch(LogEventLevel.Error);
        Logger = BuildLogger();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Log.Logger = Logger;
    }

    [SecurityCritical]
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            Logger.Fatal(ex, "Unhandled exception from AppDomain.UnhandledException");
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        Logger.Debug("Starting, Version: {Version}, CurrentDirectory: {CurrentDirectory}, CommandLine: {CommandLine}",
            GitVersionInformation.InformationalVersion,
            Environment.CurrentDirectory,
            Environment.CommandLine);
        var code = BuildCommand().Invoke(args);
        Logger.Debug("Exit code: {ExitCode}, RunAvalonia: {RunAvalonia}", code, code == CODE__AVALONIA);
        return code == CODE__AVALONIA ? Commands.RunAvaloniaReal(args) : code;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithDeveloperTools()
            .LogToTrace();

    private static RootCommand BuildCommand()
    {
        var run = new Command("run", "Run BililiveRecorder at path")
        {
            new Argument<string?>("path", () => null, "Work directory"),
            new Option<bool>("--ask-path", "Ask path in GUI even when \"don't ask again\" is selected before."),
            new Option<bool>("--hide", "Minimize to tray")
        };
        run.Handler = CommandHandler.Create((string? path, bool askPath, bool hide) =>
            Commands.RunAvaloniaHandler(path: path, askPath: askPath, hide: hide));

        var root = new RootCommand
        {
            run,
            new ToolCommand()
        };
        root.Handler = CommandHandler.Create(Commands.RunRootCommandHandler);
        return root;
    }

    private static class Commands
    {
        internal static int RunRootCommandHandler()
        {
            return RunAvaloniaHandler(path: null, askPath: false, hide: false);
        }

        internal static int RunAvaloniaHandler(string? path, bool askPath, bool hide)
        {
            Pages.RootPage.CommandArgumentRecorderPath = path;
            // Pages.RootPage.CommandArgumentFirstRun = squirrelFirstrun;
            Pages.RootPage.CommandArgumentAskPath = askPath;
            Pages.RootPage.CommandArgumentHide = hide;
            return CODE__AVALONIA;
        }

        internal static int RunAvaloniaReal(string[] args)
        {
            return BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
    }

    private static Logger BuildLogger()
    {
        var logFilePath = Environment.GetEnvironmentVariable("BILILIVERECORDER_LOG_FILE_PATH");
        if (string.IsNullOrWhiteSpace(logFilePath))
            logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "bilirec.txt");

        return new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LevelSwitchGlobal)
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithExceptionDetails()
            .Destructure.AsScalar<IPAddress>()
            .Destructure.AsScalar<ProcessingComment>()
            // .Destructure.AsScalar<StreamCodecQn>()
            .Destructure.ByTransforming<XmlFlvFile.XmlFlvFileMeta>(x => new
            {
                x.Version,
                x.ExportTime,
                x.FileSize,
                x.FileCreationTime,
                x.FileModificationTime,
            })
            .WriteTo.Console(levelSwitch: LevelSwitchConsole)
#if DEBUG
            .WriteTo.Debug()
            .WriteTo.Async(l => l.Sink<WpfLogEventSink>(LogEventLevel.Debug))
#else
                .WriteTo.Async(l => l.Sink<WpfLogEventSink>(Serilog.Events.LogEventLevel.Information))
#endif
            .WriteTo.Async(l => l.File(new CompactJsonFormatter(), logFilePath, shared: true,
                rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true))
            .CreateLogger();
    }
}
