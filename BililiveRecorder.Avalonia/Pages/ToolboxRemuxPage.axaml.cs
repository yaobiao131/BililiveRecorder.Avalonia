using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Serilog;

namespace BililiveRecorder.Avalonia.Pages;

/// <summary>
/// Interaction logic for ToolboxRemuxPage.xaml
/// </summary>
public partial class ToolboxRemuxPage : UserControl
{
    private static readonly ILogger logger = Log.ForContext<ToolboxRemuxPage>();
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    private static readonly string FFmpegWorkingDirectory;
    private static readonly string FFmpegPath;

    static ToolboxRemuxPage()
    {
        FFmpegWorkingDirectory =
            Path.Combine(Path.GetDirectoryName(typeof(ToolboxRemuxPage).Assembly.Location), "lib");
        FFmpegPath = Path.Combine(FFmpegWorkingDirectory, "miniffmpeg");
    }

    public ToolboxRemuxPage()
    {
        InitializeComponent();
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void RemuxButton_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        try
        {
            await RunAsync();
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "转封装时发生未知错误");
        }
    }

    private async Task RunAsync()
    {
        string source, target;

        {
            var suggestedStartLocation =
                await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(DesktopPath));
            var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = Lang.Resources.Toolbox_Remux_OpenFileTitle,
                AllowMultiple = false,
                SuggestedStartLocation = suggestedStartLocation,
                FileTypeFilter = [new FilePickerFileType("flv") { Patterns = ["*.flv"], MimeTypes = ["video/flv"] }]
            });
            if (files.Count <= 0)
            {
                return;
            }

            source = files[0].TryGetLocalPath() ?? string.Empty;

            // var d = new CommonOpenFileDialog()
            // {
            //     Title = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:Toolbox_Remux_OpenFileTitle"),
            //     AllowNonFileSystemItems = false,
            //     DefaultDirectory = DesktopPath,
            //     DefaultExtension = "flv",
            //     EnsureFileExists = true,
            //     EnsurePathExists = true,
            //     EnsureValidNames = true,
            //     Multiselect = false,
            // };
            //
            // d.Filters.Add(new CommonFileDialogFilter("FLV", "*.flv"));
            //
            // if (d.ShowDialog() != CommonFileDialogResult.Ok)
            //     return;
            //
            // source = d.FileName;
        }

        {
            var d = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = Lang.Resources.Toolbox_Remux_SaveFileTitle,
                SuggestedStartLocation =
                    await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
                        new Uri(Path.GetDirectoryName(source))),
                DefaultExtension = "mp4",
                FileTypeChoices = [new FilePickerFileType("mp4") { Patterns = ["*.mp4"], MimeTypes = ["video/mp4"] }],
                SuggestedFileName = Path.GetFileNameWithoutExtension(source)
            });
            if (d == null)
            {
                return;
            }

            target = d.TryGetLocalPath();
            // var d = new CommonSaveFileDialog()
            // {
            //     Title = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:Toolbox_Remux_SaveFileTitle"),
            //     AlwaysAppendDefaultExtension = true,
            //     DefaultDirectory = DesktopPath,
            //     DefaultExtension = "mp4",
            //     EnsurePathExists = true,
            //     EnsureValidNames = true,
            //     InitialDirectory = Path.GetDirectoryName(source),
            //     DefaultFileName = Path.GetFileNameWithoutExtension(source),
            // };
            //
            // d.Filters.Add(new CommonFileDialogFilter("MP4", "*.mp4"));
            //
            // if (d.ShowDialog() != CommonFileDialogResult.Ok)
            //     return;
            //
            // target = d.FileName;
        }

        logger.Debug("Remux starting, {Source}, {Target}", source, target);

//             var result = await Cli.Wrap(FFmpegPath)
//                 .WithValidation(CommandResultValidation.None)
//                 .WithWorkingDirectory(FFmpegWorkingDirectory)
//                 .WithArguments(new[] { "-hide_banner", "-loglevel", "error", "-y", "-i", source, "-c", "copy", target })
// #if DEBUG
//                 .ExecuteBufferedAsync();
// #else
//                 .ExecuteAsync();
// #endif
//
//             logger.Debug("Remux completed {@Result}", result);
    }
}
