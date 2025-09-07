using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class ContentFormat : TarsStruct
{
    public int iFontColor = -1;
    public int iFontSize = 4;
    public int iPopupStyle = 0;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iFontColor = inputStream.Read(this.iFontColor, 0, false);
        iFontSize = inputStream.Read(this.iFontSize, 1, false);
        iPopupStyle = inputStream.Read(this.iPopupStyle, 2, false);
    }
}
