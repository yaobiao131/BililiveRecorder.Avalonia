using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class MessageTagInfo: TarsStruct
{
    public int iAppId = 0;
    public string sTag = "";
    public override void WriteTo(TarsOutputStream _os)
    {
        
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iAppId = inputStream.Read(this.iAppId, 0, false);
        sTag = inputStream.Read(this.sTag, 1, false);
    }
}
