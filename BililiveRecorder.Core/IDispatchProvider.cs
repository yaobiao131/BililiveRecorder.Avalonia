namespace BililiveRecorder.Core;

public interface IDispatchProvider
{
    public void DispatchToUiThread(Action action);
}
