using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class PropView : TarsStruct
{
    public int id;
    public string name = string.Empty;
    public Dictionary<long, int> uids = new();
    public string tips = string.Empty;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        id = inputStream.Read(id, 0, false);
        name = inputStream.Read(name, 1, false);
        uids = (Dictionary<long, int>)inputStream.Read(uids, 2, false);
        tips = inputStream.Read(tips, 3, false);
    }
}
