using BililiveRecorder.Common.SimpleWebhook;
using Newtonsoft.Json;

namespace BililiveRecorder.Common.Event
{
    /// <summary>
    /// <see cref="EventType.FileOpening"/>
    /// </summary>
    public sealed class RecordFileOpeningEventArgs : RecordEventArgsBase, IRecordSessionEventArgs
    {
        public RecordFileOpeningEventArgs(IRoom room) : base(room) { }

        public Guid SessionId { get; set; }

        [JsonIgnore]
        public string FullPath { get; set; } = string.Empty;

        public string RelativePath { get; set; } = string.Empty;

        public DateTimeOffset FileOpenTime { get; set; }
    }
}
