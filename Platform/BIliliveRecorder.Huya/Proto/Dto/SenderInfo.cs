using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

internal class SenderInfo : TarsStruct
{
    public long LUid;
    public long LImid;
    public string SNickName = string.Empty;
    public int iGender;

    public override void WriteTo(TarsOutputStream os)
    {
        os.Write(LUid, 0);
        os.Write(LImid, 1);
        os.Write(SNickName, 2);
        os.Write(iGender, 3);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        LUid = inputStream.Read(LUid, 0, false);
        LImid = inputStream.Read(LImid, 1, false);
        SNickName = inputStream.Read(SNickName, 2, false);
        iGender = inputStream.Read(iGender, 3, false);
    }
}
