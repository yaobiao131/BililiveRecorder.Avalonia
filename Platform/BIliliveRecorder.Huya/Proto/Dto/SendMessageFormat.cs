using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class SendMessageFormat : TarsStruct
{
    public int iSenceType;
    public long lFormatId;
    public long lSizeTemplateId;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iSenceType = inputStream.Read(iSenceType, 0, false);
        lFormatId = inputStream.Read(lFormatId, 1, false);
        lSizeTemplateId = inputStream.Read(lSizeTemplateId, 2, false);
    }
}
