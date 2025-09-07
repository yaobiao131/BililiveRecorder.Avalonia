using System.Text;

namespace BIliliveRecorder.Huya.Tars.Tars;

internal enum TarsStructType
{
    Byte = 0,
    Short = 1,
    Int = 2,
    Long = 3,
    Float = 4,
    Double = 5,
    String1 = 6,
    String4 = 7,
    Map = 8,
    List = 9,
    StructBegin = 10,
    StructEnd = 11,
    ZeroTag = 12,
    SimpleList = 13,
}

public abstract class TarsStruct
{
    public const int TarsMaxStringLength = 100 * 1024 * 1024;

    public abstract void WriteTo(TarsOutputStream _os);
    public abstract void ReadFrom(TarsInputStream inputStream);

    public virtual void Display(StringBuilder sb, int level)
    {
    }

    public byte[] ToByteArray()
    {
        var os = new TarsOutputStream();
        WriteTo(os);
        return os.toByteArray();
    }
}
