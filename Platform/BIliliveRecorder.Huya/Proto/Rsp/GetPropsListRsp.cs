using BIliliveRecorder.Huya.Proto.Dto;
using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Rsp;

internal class GetPropsListRsp : TarsStruct
{
    public List<PropsItem> vPropsItemList = new();
    public string sMd5 = string.Empty;
    public short iNewEffectSwitch = 0;
    public short iMirrorRoomShowNum = 0;
    public short iGameRoomShowNum = 0;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        vPropsItemList = (List<PropsItem>)inputStream.Read(vPropsItemList, 1, true);
        sMd5 = inputStream.Read(sMd5, 2, true);
        iNewEffectSwitch = inputStream.Read(iNewEffectSwitch, 3, true);
        iMirrorRoomShowNum = inputStream.Read(iMirrorRoomShowNum, 4, true);
        iGameRoomShowNum = inputStream.Read(iGameRoomShowNum, 5, true);
    }
}
