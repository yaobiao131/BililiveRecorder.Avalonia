using Avalonia.Threading;
using BililiveRecorder.Core;

namespace BililiveRecorder.Avalonia;

public class AvaloniaDispatchProvider : IDispatchProvider
{
    public void DispatchToUiThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }
}
