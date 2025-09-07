using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

internal class WSMsgItem: TarsStruct
{
    public long iUri;
    public byte[] sMsg = Array.Empty<byte>();
    public long lMsgId;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iUri = inputStream.Read(iUri, 0, false);
        sMsg = inputStream.Read(sMsg, 1, false);
        lMsgId = inputStream.Read(lMsgId, 2, false);
    }
}
