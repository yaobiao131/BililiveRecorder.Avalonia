namespace BililiveRecorder.Common.Event
{
    /// <summary>
    /// <see cref="EventType.SessionStarted"/>
    /// </summary>
    public sealed class RecordSessionStartedEventArgs : RecordEventArgsBase, IRecordSessionEventArgs
    {
        public RecordSessionStartedEventArgs(IRoom room) : base(room)
        {
        }

        public Guid SessionId { get; set; }
    }
}
