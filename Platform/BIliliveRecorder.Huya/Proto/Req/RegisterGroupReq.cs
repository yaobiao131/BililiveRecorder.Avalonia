using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Req;

public class RegisterGroupReq : TarsStruct
{
    public List<string> vGroupId = new();
    public string sToken = string.Empty;

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(vGroupId, 0);
        _os.Write(sToken, 1);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
    }
}
