using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using BililiveRecorder.Avalonia.Controls;
using BililiveRecorder.ToolBox;
using BililiveRecorder.ToolBox.Tool.Analyze;
using BililiveRecorder.ToolBox.Tool.Export;
using BililiveRecorder.ToolBox.Tool.Fix;
using Serilog;

namespace BililiveRecorder.Avalonia.Pages;

public sealed class AutoFixSettings : INotifyPropertyChanged
{
    private bool splitOnScriptTag;
    private bool disableSplitOnH264AnnexB;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T location, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(location, value))
            return false;
        location = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    /// <summary>
    /// FLV修复-检测到可能缺少数据时分段
    /// </summary>
    public bool SplitOnScriptTag
    {
        get => splitOnScriptTag;
        set => SetField(ref splitOnScriptTag, value);
    }

    /// <summary>
    /// FLV修复-检测到 H264 Annex-B 时禁用修复分段
    /// </summary>
    public bool DisableSplitOnH264AnnexB
    {
        get => disableSplitOnH264AnnexB;
        set => SetField(ref disableSplitOnH264AnnexB, value);
    }
}

public partial class ToolboxAutoFixPage : UserControl
{
    private static readonly ILogger Logger = Log.ForContext<ToolboxAutoFixPage>();
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    private readonly AutoFixSettings settings = new();

    public ToolboxAutoFixPage()
    {
        InitializeComponent();
        SettingsArea.DataContext = settings;
        DragBorder.AddHandler(DragDrop.DragOverEvent, FileNameTextBox_Drop);
        DragBorder.AddHandler(DragDrop.DropEvent, FileNameTextBox_Drop);
    }

    private async void SelectFile_Button_Click(object sender, RoutedEventArgs e)
    {
        var d = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Lang.Resources.Toolbox_AutoFix_SelectInputDialog_Title,
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("FLV") { Patterns = ["*.flv"] },
                new FilePickerFileType("dev's toy") { Patterns = ["*.xml", "*.gz", "*.zip"] }
            ]
        });

        if (d.Count <= 0)
        {
            return;
        }

        FileNameTextBox.Text = d[0].TryGetLocalPath();
        // var title = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:Toolbox_AutoFix_SelectInputDialog_Title");
        // var fileDialog = new CommonOpenFileDialog()
        // {
        //     Title = title,
        //     IsFolderPicker = false,
        //     Multiselect = false,
        //     AllowNonFileSystemItems = false,
        //     AddToMostRecentlyUsedList = false,
        //     EnsurePathExists = true,
        //     EnsureFileExists = true,
        //     NavigateToShortcut = true,
        //     Filters =
        //     {
        //         new CommonFileDialogFilter("FLV",".flv"),
        //         new CommonFileDialogFilter("dev's toy",".xml,.gz,.zip")
        //     }
        // };
        // if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
        // {
        //     this.FileNameTextBox.Text = fileDialog.FileName;
        // }
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void Fix_Button_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        AutoFixProgressDialog? progressDialog = null;
        if (!semaphoreSlim.Wait(0))
            return;
        try
        {
            var inputPath = FileNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
                return;

            Logger.Debug("修复文件 {Path}", inputPath);
            progressDialog = new AutoFixProgressDialog()
            {
                CancelButtonVisibility = true,
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
                            new Uri(Path.GetDirectoryName(inputPath))),
                    SuggestedFileName = Path.GetFileName(inputPath)
                });
                if (d == null)
                    return;
                outputPath = d.TryGetLocalPath() ?? string.Empty;
            }
            var req = new FixRequest
            {
                Input = inputPath,
                OutputBase = outputPath,
                PipelineSettings = new Flv.Pipeline.ProcessingPipelineSettings
                {
                    SplitOnScriptTag = settings.SplitOnScriptTag,
                    DisableSplitOnH264AnnexB = settings.DisableSplitOnH264AnnexB,
                }
            };

            var handler = new FixHandler();
            var resp = await handler.Handle(req, token,
                    async p =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => { progressDialog.Progress = (int)(p * 98d); });
                    })
                .ConfigureAwait(true);

            Logger.Debug("修复结果 {@Response}", resp);

            if (resp.Status != ResponseStatus.Cancelled && resp.Status != ResponseStatus.OK)
            {
                Logger.Warning(resp.Exception, "修复时发生错误 {@Status}", resp.Status);
                await Task.Run(() => ShowErrorMessageBox(resp)).ConfigureAwait(true);
            }

            progressDialog.Hide();
            await showTask.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "修复时发生未处理的错误");
        }
        finally
        {
            try
            {
                Dispatcher.UIThread.Invoke((Action)(() => progressDialog?.Hide()));
                progressDialog?.CancellationTokenSource?.Cancel();
            }
            catch (Exception)
            {
                // ignored
            }

            semaphoreSlim.Release();
        }
    }

    private static async void ShowErrorMessageBox<T>(CommandResponse<T> resp) where T : IResponseData
    {
        var title = Lang.Resources.Toolbox_AutoFix_Error_Title;
        // var type = LocExtension.GetLocalizedValue<string>("BililiveRecorder.WPF:Strings:Toolbox_AutoFix_Error_Type_" + resp.Status.ToString());
        await MessageBox.Show($"{resp.ErrorMessage}", title, MessageBoxButton.OK);
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void Analyze_Button_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        AutoFixProgressDialog? progressDialog = null;

        if (!semaphoreSlim.Wait(0))
            return;
        try
        {
            var inputPath = FileNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
                return;

            Logger.Debug("分析文件 {Path}", inputPath);

            progressDialog = new AutoFixProgressDialog
            {
                CancelButtonVisibility = true,
                CancellationTokenSource = new CancellationTokenSource(),
            };
            var token = progressDialog.CancellationTokenSource.Token;
            var showTask = progressDialog.ShowAndDisableMinimizeToTrayAsync();

            var req = new AnalyzeRequest
            {
                Input = inputPath,
                PipelineSettings = new Flv.Pipeline.ProcessingPipelineSettings
                {
                    SplitOnScriptTag = settings.SplitOnScriptTag,
                    DisableSplitOnH264AnnexB = settings.DisableSplitOnH264AnnexB,
                }
            };

            var handler = new AnalyzeHandler();

            var resp = await handler.Handle(req, token,
                    async p =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => { progressDialog.Progress = (int)(p * 98d); });
                    })
                .ConfigureAwait(true);

            Logger.Debug("分析结果 {@Response}", resp);

            if (resp.Status != ResponseStatus.Cancelled)
            {
                if (resp.Status != ResponseStatus.OK)
                {
                    Logger.Warning(resp.Exception, "分析时发生错误 {@Status}", resp.Status);
                    await Task.Run(() => ShowErrorMessageBox(resp)).ConfigureAwait(true);
                }
                else
                {
                    AnalyzeResultDisplayArea.DataContext = resp.Data;
                }
            }

            progressDialog.Hide();
            await showTask.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "分析时发生未处理的错误");
        }
        finally
        {
            try
            {
                Dispatcher.UIThread.Invoke((Action)(() => progressDialog?.Hide()));
                progressDialog?.CancellationTokenSource?.Cancel();
            }
            catch (Exception)
            {
                // ignored
            }

            semaphoreSlim.Release();
        }
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    private async void Export_Button_Click(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        AutoFixProgressDialog? progressDialog = null;
        if (!semaphoreSlim.Wait(0))
            return;

        try
        {
            var inputPath = FileNameTextBox.Text;
            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
                return;

            Logger.Debug("导出文件 {Path}", inputPath);

            progressDialog = new AutoFixProgressDialog()
            {
                CancelButtonVisibility = true,
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
                            new Uri(Path.GetDirectoryName(inputPath))),
                    SuggestedFileName = Path.GetFileName(inputPath) + ".brec.xml.zip"
                });
                if (d == null)
                    return;
                outputPath = d.TryGetLocalPath() ?? string.Empty;
            }

            var req = new ExportRequest
            {
                Input = inputPath,
                Output = outputPath
            };

            var handler = new ExportHandler();

            var resp = await handler.Handle(req, token,
                    async p =>
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => { progressDialog.Progress = (int)(p * 95d); });
                    })
                .ConfigureAwait(true);

            Logger.Debug("导出分析数据结果 {@Response}", resp);

            if (resp.Status != ResponseStatus.Cancelled && resp.Status != ResponseStatus.OK)
            {
                Logger.Warning(resp.Exception, "导出分析数据时发生错误 {@Status}", resp.Status);
                await Task.Run(() => ShowErrorMessageBox(resp)).ConfigureAwait(true);
            }

            progressDialog.Hide();
            await showTask.ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "导出时发生未处理的错误");
        }
        finally
        {
            try
            {
                Dispatcher.UIThread.Invoke((Action)(() => progressDialog?.Hide()));
                progressDialog?.CancellationTokenSource?.Cancel();
            }
            catch (Exception)
            {
                // ignored
            }

            semaphoreSlim.Release();
        }
    }

    private void FileNameTextBox_Drop(object? sender, DragEventArgs e)
    {
        try
        {
            var files = e.Data.GetFiles()?.ToList();
            if (files != null && files.Count != 0)
            {
                var filenames = files.Select(f => f.TryGetLocalPath()).ToArray();
                FileNameTextBox.Text = filenames[0];
            }
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
