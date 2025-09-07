namespace BIliliveRecorder.Huya.Proto.Rsp;

public class WupRsp : BaseWup
{
    public WupRsp(byte[] vData)
    {
        Decode(vData);
    }
}
