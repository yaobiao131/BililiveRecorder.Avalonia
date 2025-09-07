using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto;

internal class WebSocketCommand : TarsStruct
{
    public int iCmdType;
    public byte[] VData = [];

    public override void WriteTo(TarsOutputStream os)
    {
        os.Write(iCmdType, 0);
        os.Write(VData, 1);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iCmdType = inputStream.Read(iCmdType, 0, false);
        VData = inputStream.Read(VData, 1, false);
    }
}
