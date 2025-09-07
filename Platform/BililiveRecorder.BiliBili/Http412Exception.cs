namespace BililiveRecorder.BiliBili;

internal class Http412Exception : Exception
{
    public Http412Exception() { }
    public Http412Exception(string message) : base(message) { }
    public Http412Exception(string message, Exception innerException) : base(message, innerException) { }
}
