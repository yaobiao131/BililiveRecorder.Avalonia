#nullable disable
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

internal class BadgeInfo : TarsStruct
{
    public long lUid;
    public long lBadgeId;
    public string sPresenterNickName = string.Empty;
    public string sBadgeName = string.Empty;
    public int iBadgeLevel;
    public int iRank;
    public int iScore;
    public int iNextScore;
    public int iQuotaUsed;
    public int iQuota;
    public long lQuotaTS;
    public long lOpenTS;
    public int iVFlag;

    public string sVLogo = string.Empty;

    public PresenterChannelInfo tChannelInfo;
    public string sPresenterLogo = string.Empty;
    public long lVExpiredTS;

    public int iBadgeType;

    public FaithInfo tFaithInfo;
    public SuperFansInfo tSuperFansInfo = new();
    public int iBaseQuota;
    public long lVConsumRank;
    public int iCustomBadgeFlag;
    public int iAgingDays;

    public int iDayScore;

    public CustomBadgeDynamicExternal tExternal;
    public int iExtinguished;
    public int iExtinguishDays;
    public int iBadgeCate;
    public int iLiveFlag;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        lUid = inputStream.Read(lUid, 0, false);
        lBadgeId = inputStream.Read(lBadgeId, 1, false);
        sPresenterNickName = inputStream.Read(sPresenterNickName, 2, false);
        sBadgeName = inputStream.Read(sBadgeName, 3, false);
        iBadgeLevel = inputStream.Read(iBadgeLevel, 4, false);
        iRank = inputStream.Read(iRank, 5, false);
        iScore = inputStream.Read(iScore, 6, false);
        iNextScore = inputStream.Read(iNextScore, 7, false);
        iQuotaUsed = inputStream.Read(iQuotaUsed, 8, false);
        iQuota = inputStream.Read(iQuota, 9, false);
        lQuotaTS = inputStream.Read(lQuotaTS, 10, false);
        lOpenTS = inputStream.Read(lOpenTS, 11, false);
        iVFlag = inputStream.Read(iVFlag, 12, false);
        sVLogo = inputStream.Read(sVLogo, 13, false);
        tChannelInfo = (PresenterChannelInfo)inputStream.Read(tChannelInfo, 14, false);
        sPresenterLogo = inputStream.Read(sPresenterLogo, 15, false);
        lVExpiredTS = inputStream.Read(lVExpiredTS, 16, false);
        iBadgeType = inputStream.Read(iBadgeType, 17, false);
        tFaithInfo = (FaithInfo)inputStream.Read(tFaithInfo, 18, false);
        tSuperFansInfo = (SuperFansInfo)inputStream.Read(tSuperFansInfo, 19, false);
        iBaseQuota = inputStream.Read(iBaseQuota, 20, false);
        lVConsumRank = inputStream.Read(lVConsumRank, 21, false);
        iCustomBadgeFlag = inputStream.Read(iCustomBadgeFlag, 22, false);
        iAgingDays = inputStream.Read(iAgingDays, 23, false);
        iDayScore = inputStream.Read(iDayScore, 24, false);
        tExternal = (CustomBadgeDynamicExternal)inputStream.Read(tExternal, 25, false);
        iExtinguished = inputStream.Read(iExtinguished, 26, false);
        iExtinguishDays = inputStream.Read(iExtinguishDays, 27, false);
        iBadgeCate = inputStream.Read(iBadgeCate, 28, false);
        iLiveFlag = inputStream.Read(iLiveFlag, 29, false);
    }
}
