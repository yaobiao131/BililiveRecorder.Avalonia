using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

internal class DecorationInfo : TarsStruct
{
    public int iAppId;
    public int iViewType;
    public byte[] vData = [];

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iAppId = inputStream.Read(iAppId, 0, false);
        iViewType = inputStream.Read(iViewType, 1, false);
        vData = inputStream.Read(vData, 2, false);
    }
}
