namespace BililiveRecorder.BiliBili.Model;

public readonly record struct StreamCodecQn
{
    public StreamCodec Codec { get; init; }
    public int Qn { get; init; }

    public override string ToString() => $"{Codec}:{Qn}";
}
