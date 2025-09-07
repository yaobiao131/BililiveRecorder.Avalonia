using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class PropsIdentity : TarsStruct
{
    public int iPropsIdType;
    public string sPropsPic18 = "";
    public string sPropsPic24 = "";
    public string sPropsPicGif = "";
    public string sPropsBannerResource = "";
    public string sPropsBannerSize = "";
    public string sPropsBannerMaxTime = "";
    public string sPropsChatBannerResource = "";
    public string sPropsChatBannerSize = "";
    public string sPropsChatBannerMaxTime = "";
    public int iPropsChatBannerPos;
    public int iPropsChatBannerIsCombo;
    public string sPropsRollContent = "";
    public int iPropsBannerAnimationstyle;
    public string sPropFaceu = "";
    public string sPropH5Resource = "";
    public string sPropsWeb = "";
    public int sWitch;
    public string sCornerMark = "";
    public int iPropViewId;
    public string sPropStreamerResource = "";
    public short iStreamerFrameRate;
    public string sPropsPic108 = "";
    public string sPcBannerResource = "";

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iPropsIdType = inputStream.Read(iPropsIdType, 1, true);
        sPropsPic18 = inputStream.Read(sPropsPic18, 2, true);
        sPropsPic24 = inputStream.Read(sPropsPic24, 3, true);
        sPropsPicGif = inputStream.Read(sPropsPicGif, 4, true);
        sPropsBannerResource = inputStream.Read(sPropsBannerResource, 5, true);
        sPropsBannerSize = inputStream.Read(sPropsBannerSize, 6, true);
        sPropsBannerMaxTime = inputStream.Read(sPropsBannerMaxTime, 7, true);
        sPropsChatBannerResource = inputStream.Read(sPropsChatBannerResource, 8, true);
        sPropsChatBannerSize = inputStream.Read(sPropsChatBannerSize, 9, true);
        sPropsChatBannerMaxTime = inputStream.Read(sPropsChatBannerMaxTime, 10, true);
        iPropsChatBannerPos = inputStream.Read(iPropsChatBannerPos, 11, true);
        iPropsChatBannerIsCombo = inputStream.Read(iPropsChatBannerIsCombo, 12, true);
        sPropsRollContent = inputStream.Read(sPropsRollContent, 13, true);
        iPropsBannerAnimationstyle = inputStream.Read(iPropsBannerAnimationstyle, 14, true);
        sPropFaceu = inputStream.Read(sPropFaceu, 15, true);
        sPropH5Resource = inputStream.Read(sPropH5Resource, 16, true);
        sPropsWeb = inputStream.Read(sPropsWeb, 17, true);
        sWitch = inputStream.Read(sWitch, 18, true);
        sCornerMark = inputStream.Read(sCornerMark, 19, true);
        iPropViewId = inputStream.Read(iPropViewId, 20, true);
        sPropStreamerResource = inputStream.Read(sPropStreamerResource, 21, true);
        iStreamerFrameRate = inputStream.Read(iStreamerFrameRate, 22, true);
        sPropsPic108 = inputStream.Read(sPropsPic108, 23, true);
        sPcBannerResource = inputStream.Read(sPcBannerResource, 24, true);
    }
}
