using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class SpecialInfo : TarsStruct
{
    public int iFirstSingle;
    public int iFirstGroup;
    public string sFirstTips = string.Empty;
    public int iSecondSingle;
    public int iSecondGroup;
    public string sSecondTips = string.Empty;
    public int iThirdSingle;
    public int iThirdGroup;
    public string sThirdTips = "";
    public int iWorldSingle;
    public int iWorldGroup;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iFirstSingle = inputStream.Read(iFirstSingle, 1, true);
        iFirstGroup = inputStream.Read(iFirstGroup, 2, true);
        sFirstTips = inputStream.Read(sFirstTips, 3, true);
        iSecondSingle = inputStream.Read(iSecondSingle, 4, true);
        iSecondGroup = inputStream.Read(iSecondGroup, 5, true);
        sSecondTips = inputStream.Read(sSecondTips, 6, true);
        iThirdSingle = inputStream.Read(iThirdSingle, 7, true);
        iThirdGroup = inputStream.Read(iThirdGroup, 8, true);
        sThirdTips = inputStream.Read(sThirdTips, 9, true);
        iWorldSingle = inputStream.Read(iWorldSingle, 10, true);
        iWorldGroup = inputStream.Read(iWorldGroup, 11, true);
    }
}
