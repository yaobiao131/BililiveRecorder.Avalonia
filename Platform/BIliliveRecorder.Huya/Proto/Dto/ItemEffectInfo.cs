using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class ItemEffectInfo : TarsStruct
{
    public int iPriceLevel;
    public int iStreamDuration;
    public int iShowType;
    public int iStreamId;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iPriceLevel = inputStream.Read(iPriceLevel, 0, false);
        iStreamDuration = inputStream.Read(iStreamDuration, 1, false);
        iShowType = inputStream.Read(iShowType, 2, false);
        iStreamId = inputStream.Read(iStreamId, 3, false);
    }
}
