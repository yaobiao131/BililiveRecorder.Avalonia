namespace BililiveRecorder.Common.Event
{
    /// <summary>
    /// <see cref="EventType.SessionEnded"/>
    /// </summary>
    public sealed class RecordSessionEndedEventArgs : RecordEventArgsBase, IRecordSessionEventArgs
    {
        public RecordSessionEndedEventArgs(IRoom room) : base(room)
        {
        }

        public Guid SessionId { get; set; }
    }
}
