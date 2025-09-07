using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class PresenterChannelInfo : TarsStruct
{
    public long lYYId;
    public long lTid;
    public long lSid;
    public int iSourceType;
    public int iScreenType;
    public long lUid;
    public int iGameId;
    public int iRoomId;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        lYYId = inputStream.Read(lYYId, 0, false);
        lTid = inputStream.Read(lTid, 1, false);
        lSid = inputStream.Read(lSid, 3, false);
        iSourceType = inputStream.Read(iSourceType, 4, false);
        iScreenType = inputStream.Read(iScreenType, 5, false);
        lUid = inputStream.Read(lUid, 6, false);
        iGameId = inputStream.Read(iGameId, 7, false);
        iRoomId = inputStream.Read(iRoomId, 8, false);
    }
}
