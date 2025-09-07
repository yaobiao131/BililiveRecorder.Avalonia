using BIliliveRecorder.Huya.Tars.Tars;

namespace BIliliveRecorder.Huya.Proto.Dto;

public class DisplayInfo : TarsStruct
{
    public int iMarqueeScopeMin;
    public int iMarqueeScopeMax;
    public int iCurrentVideoNum;
    public int iCurrentVideoMin;
    public int iCurrentVideoMax;
    public int iAllVideoNum;
    public int iAllVideoMin;
    public int iAllVideoMax;
    public int iCurrentScreenNum;
    public int iCurrentScreenMin;
    public int iCurrentScreenMax;

    public override void WriteTo(TarsOutputStream _os)
    {
    }

    public override void ReadFrom(TarsInputStream inputStream)
    {
        iMarqueeScopeMin = inputStream.Read(iMarqueeScopeMin, 1, true);
        iMarqueeScopeMax = inputStream.Read(iMarqueeScopeMax, 2, true);
        iCurrentVideoNum = inputStream.Read(iCurrentVideoNum, 3, true);
        iCurrentVideoMin = inputStream.Read(iCurrentVideoMin, 4, true);
        iCurrentVideoMax = inputStream.Read(iCurrentVideoMax, 5, true);
        iAllVideoNum = inputStream.Read(iAllVideoNum, 6, true);
        iAllVideoMin = inputStream.Read(iAllVideoMin, 7, true);
        iAllVideoMax = inputStream.Read(iAllVideoMax, 8, true);
        iCurrentScreenNum = inputStream.Read(iCurrentScreenNum, 9, true);
        iCurrentScreenMin = inputStream.Read(iCurrentScreenMin, 10, true);
        iCurrentScreenMax = inputStream.Read(iCurrentScreenMax, 11, true);
    }
}
