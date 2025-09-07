using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class FaithInfo : TarsStruct
{
    public string sFaithName = "";
    public List<FaithPresenter> vPresenter = new();

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        sFaithName = inputStream.Read(sFaithName, 0, false);
        vPresenter = (List<FaithPresenter>)inputStream.Read(vPresenter, 1, false);
    }
}
