using System.ComponentModel;

namespace BililiveRecorder.Avalonia.Models;

public class AboutModel
{
#pragma warning disable CS0067 // The event 'Recorder.PropertyChanged' is never used
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067 // The event 'Recorder.PropertyChanged' is never used

    public string InformationalVersion => GitVersionInformation.InformationalVersion;
}
