using BililiveRecorder.Common.SimpleWebhook;

namespace BililiveRecorder.Common.Event
{
    /// <summary>
    /// <see cref="EventType.StreamEnded"/>
    /// </summary>
    public sealed class StreamEndedEventArgs : RecordEventArgsBase
    {
        public StreamEndedEventArgs(IRoom room) : base(room)
        {
        }
    }
}
