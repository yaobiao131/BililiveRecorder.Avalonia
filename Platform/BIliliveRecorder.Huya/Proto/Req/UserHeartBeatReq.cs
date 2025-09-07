using BIliliveRecorder.Huya.Proto.Dto;
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Req;

public class UserHeartBeatReq : TarsStruct
{
    public UserId tId = new();
    public long lTid;
    public long lSid;
    public long lPid;
    public bool bWatchVideo;
    public int eLineType;
    public int iFps;
    public int iAttendee;
    public int iBandwidth;
    public int iLastHeartElapseTime;

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(tId, 0);
        _os.Write(lTid, 1);
        _os.Write(lSid, 2);
        _os.Write(lPid, 4);
        _os.Write(bWatchVideo, 5);
        _os.Write(eLineType, 6);
        _os.Write(iFps, 7);
        _os.Write(iAttendee, 8);
        _os.Write(iBandwidth, 9);
        _os.Write(iLastHeartElapseTime, 10);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
    }
}
