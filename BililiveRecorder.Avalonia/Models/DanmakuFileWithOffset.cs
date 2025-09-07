using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BililiveRecorder.Avalonia.Models;

internal class DanmakuFileWithOffset : INotifyPropertyChanged
{
    private string path;
    private DateTimeOffset startTime;
    private int offset;

    public string Path { get => path; set => SetField(ref path, value); }
    public DateTimeOffset StartTime { get => startTime; set => SetField(ref startTime, value); }
    public int Offset { get => offset; set => SetField(ref offset, value); }

    public DanmakuFileWithOffset(string path)
    {
        this.path = path;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    protected bool SetField<T>(ref T location, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(location, value))
            return false;
        location = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public override int GetHashCode() => HashCode.Combine(path, startTime, offset);
}
