using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Threading;
using BililiveRecorder.Core;
using Serilog.Core;
using Serilog.Events;

namespace BililiveRecorder.Avalonia;

internal class WpfLogEventSink : ILogEventSink
{
    private const int MAX_LINE = 150;
    internal static object _lock = new();
    internal static ObservableCollection<LogModel> Logs = [];

    public void Emit(LogEvent logEvent)
    {
        var msg = logEvent.RenderMessage();
        if (logEvent.Exception != null)
            msg += " " + logEvent.Exception.Message;

        var m = new LogModel
        {
            Timestamp = logEvent.Timestamp,
            Level = logEvent.Level,
            Message = msg,
        };

        if (logEvent.Properties.TryGetValue(LoggingContext.RoomId, out var propertyValue) &&
            propertyValue is ScalarValue { Value: int roomid })
        {
            m.RoomId = roomid.ToString();
        }

        var current = Application.Current;
        if (current is null)
            lock (_lock)
                AddLogToCollection(m);
        else
        {
            Dispatcher.UIThread.Post(() => AddLogToCollection(m), DispatcherPriority.Normal);
        }
    }

    private void AddLogToCollection(LogModel model)
    {
        try
        {
            Logs.Add(model);
            while (Logs.Count > MAX_LINE)
                Logs.RemoveAt(0);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public class LogModel : INotifyPropertyChanged
    {
        public DateTimeOffset Timestamp { get; set; }

        public LogEventLevel Level { get; set; }

        public string RoomId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
    }
}
