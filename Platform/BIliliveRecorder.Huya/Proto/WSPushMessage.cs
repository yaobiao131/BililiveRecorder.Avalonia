using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto;

internal class WsPushMessage : TarsStruct
{
    public int EPushType;
    public long IUri;
    public byte[] SMsg = [];
    public int IProtocolType;

    public override void WriteTo(TarsOutputStream os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        EPushType = inputStream.Read(EPushType, 0, false);
        IUri = inputStream.Read(IUri, 1, false);
        SMsg = inputStream.Read(SMsg, 2, false);
        IProtocolType = inputStream.Read(IProtocolType, 3, false);
    }
}
