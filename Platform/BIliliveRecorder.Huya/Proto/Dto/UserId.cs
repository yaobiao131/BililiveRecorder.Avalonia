using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class UserId : TarsStruct
{
    public long lUid;
    public string sGuid = string.Empty;
    public string sToken = string.Empty;
    public string sHuYaUA = string.Empty;
    public string sCookie = string.Empty;
    public int iTokenType;
    public string sDeviceInfo = string.Empty;

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(lUid, 0);
        _os.Write(sGuid, 1);
        _os.Write(sToken, 2);
        _os.Write(sHuYaUA, 3);
        _os.Write(sCookie, 4);
        _os.Write(iTokenType, 5);
        _os.Write(sDeviceInfo, 6);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        lUid = inputStream.Read(lUid, 0, false);
        sGuid = inputStream.Read(sGuid, 1, false);
        sToken = inputStream.Read(sToken, 2, false);
        sHuYaUA = inputStream.Read(sHuYaUA, 3, false);
        sCookie = inputStream.Read(sCookie, 4, false);
        iTokenType = inputStream.Read(iTokenType, 5, false);
        sDeviceInfo = inputStream.Read(sDeviceInfo, 6, false);
    }
}
