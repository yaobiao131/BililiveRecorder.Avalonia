using BililiveRecorder.Common;
using BililiveRecorder.Common.Config.V3;

namespace BililiveRecorder.Core;

internal interface IRoomFactory
{
    IRoom CreateRoom(RoomConfig roomConfig, int initDelayFactor);
}
