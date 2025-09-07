using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class SuperFansInfo : TarsStruct
{
    public long lSFExpiredTS;
    public int iSFFlag;
    public long lSFAnnualTS;
    public int iSFVariety;
    public long lOpenTS;
    public long lMemoryDay;

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(lSFExpiredTS, 0);
        _os.Write(iSFFlag, 1);
        _os.Write(lSFAnnualTS, 2);
        _os.Write(iSFVariety, 3);
        _os.Write(lOpenTS, 4);
        _os.Write(lMemoryDay, 5);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        lSFExpiredTS = inputStream.Read(lSFExpiredTS, 0, false);
        iSFFlag = inputStream.Read(iSFFlag, 1, false);
        lSFAnnualTS = inputStream.Read(lSFAnnualTS, 2, false);
        iSFVariety = inputStream.Read(iSFVariety, 3, false);
        lOpenTS = inputStream.Read(lOpenTS, 4, false);
        lMemoryDay = inputStream.Read(lMemoryDay, 5, false);
    }
}
