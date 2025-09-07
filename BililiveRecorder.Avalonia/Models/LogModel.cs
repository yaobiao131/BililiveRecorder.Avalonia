using System.Collections.ObjectModel;

namespace BililiveRecorder.Avalonia.Models;

internal class LogModel() : ReadOnlyObservableCollection<WpfLogEventSink.LogModel>(WpfLogEventSink.Logs);
