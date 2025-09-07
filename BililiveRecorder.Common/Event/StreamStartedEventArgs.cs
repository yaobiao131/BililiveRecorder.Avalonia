namespace BililiveRecorder.Common.Event
{
    /// <summary>
    /// <see cref="EventType.StreamStarted"/>
    /// </summary>
    public sealed class StreamStartedEventArgs : RecordEventArgsBase
    {
        public StreamStartedEventArgs(IRoom room) : base(room)
        {
        }
    }
}
