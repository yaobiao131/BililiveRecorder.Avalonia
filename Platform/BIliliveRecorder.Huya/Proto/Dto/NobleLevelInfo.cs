using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class NobleLevelInfo : TarsStruct
{
    public int iNobleLevel;
    public int iAttrType;

    public override void WriteTo(TarsOutputStream _os)
    {
        _os.Write(iNobleLevel, 0);
        _os.Write(iAttrType, 1);
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iNobleLevel = inputStream.Read(iNobleLevel, 0, true);
        iAttrType = inputStream.Read(iAttrType, 1, true);
    }
}
