using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;

namespace BililiveRecorder.Avalonia.Controls;

public partial class WorkDirectorySelectorDialog : ContentDialog, INotifyPropertyChanged
{
    private WorkDirectorySelectorDialogError error = WorkDirectorySelectorDialogError.None;
    private string path = string.Empty;
    private bool skipAsking;

    protected override Type StyleKeyOverride { get; } = typeof(ContentDialog);

    public WorkDirectorySelectorDialogError Error
    {
        get => error;
        set => SetField(ref error, value);
    }

    public string Path
    {
        get => path;
        set => SetField(ref path, value);
    }

    public bool SkipAsking
    {
        get => skipAsking;
        set => SetField(ref skipAsking, value);
    }

    public WorkDirectorySelectorDialog()
    {
        DataContext = this;
        InitializeComponent();
    }

    public enum WorkDirectorySelectorDialogError
    {
        None,
        UnknownError,
        PathNotSupported,
        PathDoesNotExist,
        PathContainsFiles,
        FailedToLoadConfig,
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var folders = await App.TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Lang.Resources.WorkDirectorySelector_Title,
            AllowMultiple = false,
            SuggestedStartLocation = string.IsNullOrWhiteSpace(path) ? null : await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(path))
        });

        if (folders.Count <= 0)
        {
            return;
        }

        var localPath = folders[0].TryGetLocalPath();
        if (localPath == null) return;
        Path = localPath;
    }
}
