#nullable disable
using BIliliveRecorder.Huya.Proto.Dto;
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto;

internal class SendItemSubBroadcastPacket : TarsStruct
{
    #region MyRegion

    public int iItemType;
    public string strPayId = "";
    public int iItemCount;
    public long lPresenterUid;
    public long lSenderUid;
    public string sPresenterNick = "";
    public string sSenderNick = "";
    public string sSendContent = "";
    public int iItemCountByGroup;
    public int iItemGroup;
    public int iSuperPupleLevel;
    public int iComboScore;
    public int iDisplayInfo;
    public int iEffectType;
    public string iSenderIcon = "";
    public string iPresenterIcon = "";
    public int iTemplateType;
    public string sExpand = "";
    public bool bBusi;
    public int iColorEffectType;
    public string sPropsName = "";
    public short iAccpet;

    public short iEventType;

    public UserIdentityInfo userInfo = new();
    public long lRoomId;

    public long lHomeOwnerUid;

    //    private int streamerInfo = new D.StreamerNode;
    public int iPayType = -1;

    public int iNobleLevel;

    public NobleLevelInfo tNobleLevel = new();

    public ItemEffectInfo tEffectInfo = new();
    public List<long> vExUid = new();
    public int iComboStatus;
    public int iPidColorType;
    public int iMultiSend;
    public int iVFanLevel;
    public int iUpgradeLevel;

    public string sCustomText = "";

    public DIYBigGiftEffect tDIYEffect = new();
    public long lComboSeqId;
    public long lPayTotal;
//    private int vBizData = new V.Vector(new D.ItemEffectBizData);

    public BadgeInfo badgeInfo;
    // private PropsItem propsItem = PropsItem.DEFAULT;

    #endregion

    public override void WriteTo(TarsOutputStream os)
    {
        throw new NotImplementedException();
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iItemType = inputStream.Read(iItemType, 0, true);
        strPayId = inputStream.Read(strPayId, 1, true);
        iItemCount = inputStream.Read(iItemCount, 2, true);
        lPresenterUid = inputStream.Read(lPresenterUid, 3, true);
        lSenderUid = inputStream.Read(lSenderUid, 4, true);
        sPresenterNick = inputStream.Read(sPresenterNick, 5, true);
        sSenderNick = inputStream.Read(sSenderNick, 6, true);
        sSendContent = inputStream.Read(sSendContent, 7, true);
        iItemCountByGroup = inputStream.Read(iItemCountByGroup, 8, true);
        iItemGroup = inputStream.Read(iItemGroup, 9, true);
        iSuperPupleLevel = inputStream.Read(iSuperPupleLevel, 10, true);
        iComboScore = inputStream.Read(iComboScore, 11, true);
        iDisplayInfo = inputStream.Read(iDisplayInfo, 12, true);
        iEffectType = inputStream.Read(iEffectType, 13, true);
        iSenderIcon = inputStream.Read(iSenderIcon, 14, true);
        iPresenterIcon = inputStream.Read(iPresenterIcon, 15, true);
        iTemplateType = inputStream.Read(iTemplateType, 16, true);
        sExpand = inputStream.Read(sExpand, 17, true);
        bBusi = inputStream.Read(bBusi, 18, true);
        iColorEffectType = inputStream.Read(iColorEffectType, 19, true);
        sPropsName = inputStream.Read(sPropsName, 20, true);
        iAccpet = inputStream.Read(iAccpet, 21, true);
        iEventType = inputStream.Read(iEventType, 22, true);
        userInfo = (UserIdentityInfo)inputStream.Read(userInfo, 23, true);
        lRoomId = inputStream.Read(lRoomId, 24, true);
        lHomeOwnerUid = inputStream.Read(lHomeOwnerUid, 25, true);
//        this.streamerInfo = _is.Read(this.streamerInfo, 26, true);
        iPayType = inputStream.Read(iPayType, 27, true);
        iNobleLevel = inputStream.Read(iNobleLevel, 28, true);
        tNobleLevel = (NobleLevelInfo)inputStream.Read(tNobleLevel, 29, true);
        tEffectInfo = (ItemEffectInfo)inputStream.Read(tEffectInfo, 30, true);
        vExUid = (List<long>)inputStream.Read(vExUid, 31, true);
        iComboStatus = inputStream.Read(iComboStatus, 32, true);
        iPidColorType = inputStream.Read(iPidColorType, 33, true);
        iMultiSend = inputStream.Read(iMultiSend, 34, true);
        iVFanLevel = inputStream.Read(iVFanLevel, 35, true);
        iUpgradeLevel = inputStream.Read(iUpgradeLevel, 36, true);
        sCustomText = inputStream.Read(sCustomText, 37, true);
        tDIYEffect = (DIYBigGiftEffect) inputStream.Read(tDIYEffect, 38, true);
        lComboSeqId = inputStream.Read(lComboSeqId, 39, true);
        lPayTotal = inputStream.Read(lPayTotal, 41, true);
//        this.vBizData = _is.Read(this.vBizData, 42, true);
    }
}
