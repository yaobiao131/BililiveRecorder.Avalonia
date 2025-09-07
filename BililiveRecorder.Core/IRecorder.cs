using System.Collections.ObjectModel;
using System.ComponentModel;
using BililiveRecorder.Common;
using BililiveRecorder.Common.Config.V3;
using BililiveRecorder.Common.Event;

namespace BililiveRecorder.Core;

public interface IRecorder : INotifyPropertyChanged, IDisposable
{
    ConfigV3 Config { get; }
    ReadOnlyObservableCollection<IRoom> Rooms { get; }

    event EventHandler<AggregatedRoomEventArgs<RecordSessionStartedEventArgs>>? RecordSessionStarted;
    event EventHandler<AggregatedRoomEventArgs<RecordSessionEndedEventArgs>>? RecordSessionEnded;
    event EventHandler<AggregatedRoomEventArgs<RecordFileOpeningEventArgs>>? RecordFileOpening;
    event EventHandler<AggregatedRoomEventArgs<RecordFileClosedEventArgs>>? RecordFileClosed;
    event EventHandler<AggregatedRoomEventArgs<IOStatsEventArgs>>? IOStats;
    event EventHandler<AggregatedRoomEventArgs<RecordingStatsEventArgs>>? RecordingStats;
    event EventHandler<IRoom> StreamStarted;

    IRoom AddRoom(long roomid, Platform platform);
    IRoom AddRoom(long roomid, bool enabled, Platform platform);
    void RemoveRoom(IRoom room);

    void SaveConfig();
}
