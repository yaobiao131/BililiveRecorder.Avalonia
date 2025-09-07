using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class CustomBadgeDynamicExternal: TarsStruct
{
    public string sFloorExter = "";
    public int iFansIdentity;
    public override void WriteTo(TarsOutputStream _os)
    {
        
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        sFloorExter = inputStream.Read(sFloorExter, 0, false);
        iFansIdentity = inputStream.Read(iFansIdentity, 1, false);
    }
}
