using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using BililiveRecorder.Avalonia.Controls;
using BililiveRecorder.Avalonia.Models;
using BililiveRecorder.ToolBox;
using BililiveRecorder.ToolBox.Tool.DanmakuMerger;
using BililiveRecorder.ToolBox.Tool.DanmakuStartTime;
using FluentAvalonia.UI.Data;
using Serilog;

namespace BililiveRecorder.Avalonia.Pages;

public partial class ToolboxDanmakuMergerPage : UserControl
{
    private static readonly ILogger logger = Log.ForContext<ToolboxDanmakuMergerPage>();
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    private readonly ObservableCollection<DanmakuFileWithOffset> Files = [];

    public ToolboxDanmakuMergerPage()
    {
        InitializeComponent();

        var cvs = (CollectionViewSource?)this.FindResource("cvs");
        if (cvs != null) cvs.Source = Files;
        FilesGrid.AddHandler(DragDrop.DragOverEvent, OnDragDrop);
        FilesGrid.AddHandler(DragDrop.DropEvent, OnDragDrop);
    }

    private void RemoveFile_Click(object sender, RoutedEventArgs e)
    {
        var b = (Button)sender;
        var f = (DanmakuFileWithOffset?)b.DataContext;
        if (f == null)
            return;
        Files.Remove(f);

        CalculateOffsets();
    }
#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void OnDragDrop(object? sender, DragEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        try
        {
            var files = e.Data.GetFiles()?.ToList();
            if (files != null && files.Count != 0)
            {
                await AddFilesAsync(files.Select(x => x.Name)
                    .Where(x => x.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)).ToArray());
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void AddFile_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        var suggestedStartLocation =
            await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(DesktopPath));
        var d = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang.Resources.Toolbox_Merge_OpenFileDialogTitle,
            AllowMultiple = true,
            SuggestedStartLocation = suggestedStartLocation,
            FileTypeFilter =
                [new FilePickerFileType(Lang.Resources.Toolbox_Merge_XmlDanmakuFiles) { Patterns = ["*.xml"] }]
        });

        if (d.Count <= 0)
        {
            return;
        }

        // var d = new CommonOpenFileDialog
        // {
        //     Title = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:Toolbox_Merge_OpenFileDialogTitle"),
        //     AllowNonFileSystemItems = false,
        //     EnsureFileExists = true,
        //     EnsurePathExists = true,
        //     EnsureValidNames = true,
        //     DefaultDirectory = DesktopPath,
        //     DefaultExtension = "xml",
        //     Multiselect = true,
        // };
        //
        // d.Filters.Add(new CommonFileDialogFilter(LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:Toolbox_Merge_XmlDanmakuFiles"), "*.xml"));
        //
        // if (d.ShowDialog() != CommonFileDialogResult.Ok)
        //     return;

        await AddFilesAsync(d.Select(x => x.TryGetLocalPath()).ToArray());
    }

    private async Task AddFilesAsync(string[] paths)
    {
        // filter duplicate file paths
        var req = new DanmakuStartTimeRequest
            { Inputs = paths.Where(x => Files.All(f => f.Path != x)).ToArray() };
        var handler = new DanmakuStartTimeHandler();
        var resp = await handler.Handle(req, default, default).ConfigureAwait(true);

        if (resp.Status != ResponseStatus.OK || resp.Data is null)
            return;

        var toBeAdded = resp.Data.StartTimes.Select(x => new DanmakuFileWithOffset(x.Path) { StartTime = x.StartTime });
        foreach (var file in toBeAdded)
            Files.Add(file);

        Dispatcher.UIThread.Invoke((Action)CalculateOffsets, DispatcherPriority.Normal);
    }

    private void CalculateOffsets()
    {
        if (Files.Count == 0)
            return;

        var minTime = Files.Min(x => x.StartTime);

        foreach (var item in Files)
        {
            item.Offset = (int)(item.StartTime - minTime).TotalSeconds;
        }
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void Merge_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        AutoFixProgressDialog? progressDialog = null;
        try
        {
            var inputPaths = Files.Distinct().ToArray();

            if (inputPaths.Length < 2)
            {
                await MessageBox.Show(
                    Lang.Resources.Toolbox_Merge_Error_AtLeastTwo,
                    Lang.Resources.Toolbox_Merge_Title,
                    MessageBoxButton.OK);
                return;
            }

            logger.Debug("合并弹幕文件 {Paths}", inputPaths);

            progressDialog = new AutoFixProgressDialog
            {
                CancelButtonVisibility = false,
                CancellationTokenSource = new CancellationTokenSource(),
            };
            var token = progressDialog.CancellationTokenSource.Token;
            var showTask = progressDialog.ShowAndDisableMinimizeToTrayAsync();

            string outputPath;
            {
                var d = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = Lang.Resources.Toolbox_AutoFix_SelectOutputDialog_Title,
                    SuggestedStartLocation =
                        await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
                            new Uri(Path.GetDirectoryName(inputPaths[0].Path))),
                    DefaultExtension = "xml",
                    FileTypeChoices =
                        [new FilePickerFileType(Lang.Resources.Toolbox_Merge_XmlDanmakuFiles) { Patterns = ["*.xml"] }]
                });
                if (d == null)
                    return;
                outputPath = d.TryGetLocalPath() ?? string.Empty;
            }

            var req = new DanmakuMergerRequest
            {
                Inputs = inputPaths.Select(x => x.Path).ToArray(),
                Offsets = inputPaths.Select(x => x.Offset).ToArray(),
                Output = outputPath,
            };

            var handler = new DanmakuMergerHandler();

            var resp = await handler.Handle(req, token,
                    async p =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => { progressDialog.Progress = (int)(p * 98d); });
                    })
                .ConfigureAwait(true);

            logger.Debug("弹幕合并结果 {@Response}", resp);

            if (resp.Status is not ResponseStatus.Cancelled and not ResponseStatus.OK)
            {
                logger.Warning(resp.Exception, "弹幕合并时发生错误 {@Status}", resp.Status);
                await Task.Run(() => ShowErrorMessageBox(resp), token).ConfigureAwait(true);
            }
            else
            {
                Files.Clear();
            }

            progressDialog.Hide();
            await showTask.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "弹幕合并时发生未处理的错误");
        }
        finally
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync((Action)(() => progressDialog?.Hide()));
                progressDialog?.CancellationTokenSource?.Cancel();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private static async void ShowErrorMessageBox<T>(CommandResponse<T> resp) where T : IResponseData
    {
        var title = Lang.Resources.Toolbox_AutoFix_Error_Title;
        // var type = Lang.Resources.Toolbox_AutoFix_Error_Type_ + resp.Status.ToString();
        await MessageBox.Show($"{resp.ErrorMessage}", title, MessageBoxButton.OK);
    }
}
