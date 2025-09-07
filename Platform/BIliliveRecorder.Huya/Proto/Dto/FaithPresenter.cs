using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class FaithPresenter : TarsStruct
{
    public long lPid;
    public string sLogo = "";

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        lPid = inputStream.Read(lPid, 0, false);
        sLogo = inputStream.Read(sLogo, 1, false);
    }
}
