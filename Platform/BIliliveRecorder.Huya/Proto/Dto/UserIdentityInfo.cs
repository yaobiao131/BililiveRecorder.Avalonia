using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class UserIdentityInfo : TarsStruct
{
    private List<DecorationInfo> vDecorationPrefix = new();
    private List<DecorationInfo> vDecorationSuffix = new();

    public override void WriteTo(TarsOutputStream os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        vDecorationPrefix = (List<DecorationInfo>)inputStream.Read(vDecorationPrefix, 0, false);
        vDecorationSuffix = (List<DecorationInfo>)inputStream.Read(vDecorationSuffix, 1, false);
    }
}
