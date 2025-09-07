using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class BulletFormat : TarsStruct
{
    public int iFontColor = -1;
    public int iFontSize = 4;
    public int iTextSpeed = 0;
    public int iTransitionType = 1;
    public int iPopupStyle = 0;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iFontColor = inputStream.Read(iFontColor, 0, false);
        iFontSize = inputStream.Read(iFontSize, 1, false);
        iTextSpeed = inputStream.Read(iTextSpeed, 2, false);
        iTransitionType = inputStream.Read(iTransitionType, 3, false);
        iPopupStyle = inputStream.Read(iPopupStyle, 4, false);
    }
}
