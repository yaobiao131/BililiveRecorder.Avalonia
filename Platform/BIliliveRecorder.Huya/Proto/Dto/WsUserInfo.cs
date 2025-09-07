using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

internal class WsUserInfo : TarsStruct
{
    public long LUid = 0;
    public bool BAnonymous = true;
    public string SGuid = string.Empty;
    public string SToken = string.Empty;
    public long LTid;
    public long LSid;
    public long LGroupId;
    public long LGroupType;
    public string SAppId = string.Empty;
    public string SUa = string.Empty;

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(LUid, 0);
        _os.Write(BAnonymous, 1);
        _os.Write(SGuid, 2);
        _os.Write(SToken, 3);
        _os.Write(LTid, 4);
        _os.Write(LSid, 5);
        _os.Write(LGroupId, 6);
        _os.Write(LGroupType, 7);
        _os.Write(SAppId, 8);
        _os.Write(SUa, 9);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        LUid = inputStream.Read(LUid, 0, false);
        BAnonymous = inputStream.Read(BAnonymous, 1, false);
        SGuid = inputStream.Read(Guid.Empty.ToString(), 2, false);
        SToken = inputStream.Read(string.Empty, 3, false);
        LTid = inputStream.Read(LTid, 4, false);
        LSid = inputStream.Read(LSid, 5, false);
        LGroupId = inputStream.Read(LGroupId, 6, false);
        LGroupType = inputStream.Read(LGroupType, 7, false);
        SAppId = inputStream.Read(string.Empty, 8, false);
        SUa = inputStream.Read(string.Empty, 9, false);
    }
}
