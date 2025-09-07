using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class DIYBigGiftEffect : TarsStruct
{
    public string sResourceUrl = "";
    public string sResourceAttr = "";
    public string sWebResourceUrl = "";
    public string sPCResourceUrl = "";

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        sResourceUrl = inputStream.Read(sResourceUrl, 0, false);
        sResourceAttr = inputStream.Read(sResourceAttr, 1, false);
        sWebResourceUrl = inputStream.Read(sWebResourceUrl, 2, false);
        sPCResourceUrl = inputStream.Read(sPCResourceUrl, 3, false);
    }
}
