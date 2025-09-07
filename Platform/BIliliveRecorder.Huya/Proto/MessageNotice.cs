using BIliliveRecorder.Huya.Proto.Dto;
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto;

internal class MessageNotice : TarsStruct
{
    public SenderInfo tUserInfo = new();
    public long lTid;
    public long lSid;
    public string sContent = string.Empty;

    public int iShowMode;

    public ContentFormat tFormat = new();
    public BulletFormat tBulletFormat = new();
    public int iTermType;
    public List<DecorationInfo> vDecorationPrefix = [];
    public List<DecorationInfo> vDecorationSuffix = [];
    public long lPid;

    public override void WriteTo(TarsOutputStream os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        tUserInfo = (SenderInfo)inputStream.Read(tUserInfo, 0, false);
        lTid = inputStream.Read(lTid, 1, false);
        lSid = inputStream.Read(lSid, 2, false);
        sContent = inputStream.Read(sContent, 3, false);
        iShowMode = inputStream.Read(iShowMode, 4, false);
        tFormat = (ContentFormat)inputStream.Read(tFormat, 5, false);
        tBulletFormat = (BulletFormat)inputStream.Read(tBulletFormat, 6, false);
        iTermType = inputStream.Read(iTermType, 7, false);
        vDecorationPrefix = (List<DecorationInfo>)inputStream.Read(vDecorationPrefix, 8, false);
        vDecorationSuffix = (List<DecorationInfo>)inputStream.Read(vDecorationSuffix, 9, false);
        lPid = inputStream.Read(lPid, 11, false);
    }
}
