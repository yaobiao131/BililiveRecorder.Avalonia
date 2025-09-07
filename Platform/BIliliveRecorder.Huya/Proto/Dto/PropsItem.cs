using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

internal class PropsItem : TarsStruct
{
    public int iPropsId;
    public string sPropsName = "";
    public int iPropsYb;
    public int iPropsGreenBean;
    public int iPropsWhiteBean;
    public int iPropsGoldenBean;
    public int iPropsRed;
    public int iPropsPopular;
    public int iPropsExpendNum = -1;
    public int iPropsFansValue = -1;
    public List<int> vPropsNum = new();
    public int iPropsMaxNum;
    public int iPropsBatterFlag;
    public List<int> vPropsChannel = new();
    public string sPropsToolTip = string.Empty;
    public List<PropsIdentity> vPropsIdentity = new();
    public int iPropsWeights;

    public int iPropsLevel;

    public DisplayInfo tDisplayInfo = new();
    public SpecialInfo tSpecialInfo = new();
    public int iPropsGrade;
    public int iPropsGroupNum;
    public string sPropsCommBannerResource = string.Empty;
    public string sPropsOwnBannerResource = string.Empty;
    public int iPropsShowFlag;
    public int iTemplateType;
    public int iShelfStatus;
    public string sAndroidLogo = string.Empty;
    public string sIpadLogo = string.Empty;
    public string sIphoneLogo = string.Empty;
    public string sPropsCommBannerResourceEx = string.Empty;
    public string sPropsOwnBannerResourceEx = string.Empty;

    public List<long> vPresenterUid = new();

    public List<PropView> vPropView = new();
    public short iFaceUSwitch;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iPropsId = inputStream.Read(iPropsId, 1, true);
        sPropsName = inputStream.Read(sPropsName, 2, true);
        iPropsYb = inputStream.Read(iPropsYb, 3, true);
        iPropsGreenBean = inputStream.Read(iPropsGreenBean, 4, true);
        iPropsWhiteBean = inputStream.Read(iPropsWhiteBean, 5, true);
        iPropsGoldenBean = inputStream.Read(iPropsGoldenBean, 6, true);
        iPropsRed = inputStream.Read(iPropsRed, 7, true);
        iPropsPopular = inputStream.Read(iPropsPopular, 8, true);
        iPropsExpendNum = inputStream.Read(iPropsExpendNum, 9, true);
        iPropsFansValue = inputStream.Read(iPropsFansValue, 10, true);
        vPropsNum = (List<int>)inputStream.Read(vPropsNum, 11, true);
        iPropsMaxNum = inputStream.Read(iPropsMaxNum, 12, true);
        iPropsBatterFlag = inputStream.Read(iPropsBatterFlag, 13, true);
        vPropsChannel = (List<int>)inputStream.Read(vPropsChannel, 14, true);
        sPropsToolTip = inputStream.Read(sPropsToolTip, 15, true);
        vPropsIdentity = (List<PropsIdentity>)inputStream.Read(vPropsIdentity, 16, true);
        iPropsWeights = inputStream.Read(iPropsWeights, 17, true);
        iPropsLevel = inputStream.Read(iPropsLevel, 18, true);
        tDisplayInfo = (DisplayInfo)inputStream.Read(tDisplayInfo, 19, true);
        tSpecialInfo = (SpecialInfo)inputStream.Read(tSpecialInfo, 20, true);
        iPropsGrade = inputStream.Read(iPropsGrade, 21, true);
        iPropsGroupNum = inputStream.Read(iPropsGroupNum, 22, true);
        sPropsCommBannerResource = inputStream.Read(sPropsCommBannerResource, 23, true);
        sPropsOwnBannerResource = inputStream.Read(sPropsOwnBannerResource, 24, true);
        iPropsShowFlag = inputStream.Read(iPropsShowFlag, 25, true);
        iTemplateType = inputStream.Read(iTemplateType, 26, true);
        iShelfStatus = inputStream.Read(iShelfStatus, 27, true);
        sAndroidLogo = inputStream.Read(sAndroidLogo, 28, true);
        sIpadLogo = inputStream.Read(sIpadLogo, 29, true);
        sIphoneLogo = inputStream.Read(sIphoneLogo, 30, true);
        sPropsCommBannerResourceEx = inputStream.Read(sPropsCommBannerResourceEx, 31, true);
        sPropsOwnBannerResourceEx = inputStream.Read(sPropsOwnBannerResourceEx, 32, true);
        vPresenterUid = (List<long>)inputStream.Read(vPresenterUid, 33, true);
        vPropView = (List<PropView>)inputStream.Read(vPropView, 34, true);
        iFaceUSwitch = inputStream.Read(iFaceUSwitch, 35, true);
    }
}
