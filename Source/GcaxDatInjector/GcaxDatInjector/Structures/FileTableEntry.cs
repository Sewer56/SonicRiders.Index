using System.Runtime.CompilerServices;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;

namespace GcaxDatInjector.Structures;

public unsafe struct FileTableEntry
{
    public const int AdpcmCoefficientCount = 16;

    /// <summary>
    /// Offset to raw data relative to DtpkHeader.pcmdSectionPtr
    /// </summary>
    public int PcmdOffset;

    /// <summary>
    /// Start address. Equivalent of Nintendo DSP header value.
    /// </summary>
    public int CurrentAddress;

    /// <summary>
    /// Usually (size * 2) +- 1. Strongly believe this is sample count.
    /// </summary>
    public int NumNibbles;

    /// <summary>
    /// I'm not an audio engineer. Ask Yacker.
    /// </summary>
    public fixed short AdpcmCoefficients[AdpcmCoefficientCount];

    /// <summary>
    /// Might be some sort of starting sample.
    /// </summary>
    public int MaybePredictorScale;

    public int Unk4;
    public int Unk5;

    /// <summary>
    /// Usually constant value of 512.
    /// </summary>
    public int UnknownConst512;

    /// <summary>
    /// Sample rate of the audio.
    /// </summary>
    public ushort SampleRate;

    /// <summary>
    /// Padding
    /// </summary>
    public short Padding;
    
    /// <summary>
    /// Size of the file in bytes. In the PCMD section.
    /// </summary>
    public int Size;

    public FileTableEntry(in DspHeader header) : this()
    {
        FromDspHeader(header);
    }

    public void FromDspHeader(in DspHeader header)
    {
        NumNibbles = (int)header.AdpcmNibbles;
        CurrentAddress = (int)header.CurrentAddress;
        MaybePredictorScale = header.PredictorScale;

        for (int x = 0; x < FileTableEntry.AdpcmCoefficientCount; x++)
            AdpcmCoefficients[x] = header.AdpcmCoefficients[x];

        UnknownConst512 = 512;
        SampleRate = (ushort)header.SampleRate;
    }

    [SkipLocalsInit]
    public static FileTableEntry Read(EndianStreamReader reader)
    {
        var entry = new FileTableEntry();
        reader.Read(out entry.PcmdOffset);
        reader.Read(out entry.CurrentAddress);
        reader.Read(out entry.NumNibbles);

        for (int x = 0; x < AdpcmCoefficientCount; x++)
            reader.Read(out entry.AdpcmCoefficients[x]);

        reader.Read(out entry.MaybePredictorScale);
        reader.Read(out entry.Unk4);
        reader.Read(out entry.Unk5);

        reader.Read(out entry.UnknownConst512);
        reader.Read(out entry.SampleRate);
        reader.Read(out entry.Padding);
        reader.Read(out entry.Size);

        return entry;
    }

    public void Write(Stream output)
    {
        output.WriteBigEndianPrimitive(PcmdOffset);
        output.WriteBigEndianPrimitive(CurrentAddress);
        output.WriteBigEndianPrimitive(NumNibbles);

        for (int x = 0; x < AdpcmCoefficientCount; x++)
            output.WriteBigEndianPrimitive(AdpcmCoefficients[x]);

        output.WriteBigEndianPrimitive(MaybePredictorScale);
        output.WriteBigEndianPrimitive(Unk4);
        output.WriteBigEndianPrimitive(Unk5);

        output.WriteBigEndianPrimitive(UnknownConst512);
        output.WriteBigEndianPrimitive(SampleRate);
        output.WriteBigEndianPrimitive(Padding);
        output.WriteBigEndianPrimitive(Size);
    }
}