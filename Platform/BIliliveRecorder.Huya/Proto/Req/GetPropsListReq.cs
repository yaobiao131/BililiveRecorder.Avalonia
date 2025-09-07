using BIliliveRecorder.Huya.Proto.Dto;
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Req;

internal class GetPropsListReq : TarsStruct
{
    public UserId tUserId = new();
    public string sMd5 = string.Empty;
    public int iTemplateType;
    public string sVersion = string.Empty;
    public int iAppId;
    public long lPresenterUid;
    public long lSid;
    public long lSubSid;

    public override void WriteTo(TarsOutputStream os)
    {
        os.Write(tUserId, 1);
        os.Write(sMd5, 2);
        os.Write(iTemplateType, 3);
        os.Write(sVersion, 4);
        os.Write(iAppId, 5);
        os.Write(lPresenterUid, 6);
        os.Write(lSid, 7);
        os.Write(lSubSid, 8);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
    }
}
