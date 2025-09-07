#nullable disable
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

internal class VipEnterBanner : TarsStruct
{
    public long lUid = 0;
    public string sNickName;

    public long lPid = 0;

    // NobleInfo tNobleInfo;
    // GuardInfo tGuardInfo;
    // WeekRankInfo tWeekRankInfo;
    public string sLogoURL;
    public bool bFromNearby = false;

    public string sLocation;

    // DecorationInfoRsp tDecorationInfo;
    // WeekRankInfo tWeekHeartStirRankInfo;
    // WeekRankInfo tWeekHeartBlockRankInfo;
    public int iMasterRank = 0;

    // ACEnterBanner tACInfo;
    // std::vector<CommEnterBanner> vCommEnterBanner;
    // UserRidePetInfo tRidePetInfo;
    public override void WriteTo(TarsOutputStream os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        lUid = inputStream.Read(lUid, 0, false);
        sNickName = inputStream.Read(sNickName, 1, false);
        lPid = inputStream.Read(lPid, 2, false);
        // stream.read(tNobleInfo, 3, false);
        // stream.read(tGuardInfo, 4, false);
        // stream.read(tWeekRankInfo, 5, false);
        sLogoURL = inputStream.Read(sLogoURL, 6, false);
        bFromNearby = inputStream.Read(bFromNearby, 7, false);
        sLocation = inputStream.Read(sLocation, 8, false);
        // stream.read(tDecorationInfo, 9, false);
        // stream.read(tWeekHeartStirRankInfo, 10, false);
        // stream.read(tWeekHeartBlockRankInfo, 11, false);
        iMasterRank = inputStream.Read(iMasterRank, 12, false);
        // stream.read(tACInfo, 13, false);
        // stream.read(vCommEnterBanner, 14, false);
        // stream.read(tRidePetInfo, 15, false);
    }
}
