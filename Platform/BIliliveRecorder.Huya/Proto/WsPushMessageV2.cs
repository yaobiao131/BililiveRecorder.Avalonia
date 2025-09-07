using BIliliveRecorder.Huya.Proto.Dto;
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto;

internal class WsPushMessageV2 : TarsStruct
{
    public string SGroupId = string.Empty;
    public List<WSMsgItem> VMsgItem = [];

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        SGroupId = inputStream.Read(SGroupId, 0, false);
        VMsgItem = (List<WSMsgItem>)inputStream.Read(VMsgItem, 1, false);
    }
}
