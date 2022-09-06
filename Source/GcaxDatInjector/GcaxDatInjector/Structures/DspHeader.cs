using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;
using Reloaded.Memory.Streams.Writers;
using VGAudio.Codecs.GcAdpcm;

namespace GcaxDatInjector.Structures;

/// <summary>
/// Header of a Nintendo DSP file.
/// </summary>
public unsafe struct DspHeader
{
    private const int PadShorts = 11;

    /// <summary>
    /// Number of samples in audio data.
    /// </summary>
    public uint NumSamples; // 0
    
    /// <summary>
    /// Including Headers.
    /// </summary>
    public uint AdpcmNibbles; // 4

    /// <summary>
    /// Sample rate, in Hz.
    /// </summary>
    public uint SampleRate; // 8

    // Looping
    public ushort LoopFlag; // 12
    public ushort Format;   // 14

    public uint LoopStartOffset; // 16
    public uint LoopEndOffset;   // 20

    /// <summary>
    /// Always zero.
    /// </summary>
    public uint CurrentAddress; // 24

    /// <summary>
    /// I'm not an audio engineer. Ask Yacker.
    /// </summary>
    public fixed short AdpcmCoefficients[FileTableEntry.AdpcmCoefficientCount]; // 26

    // Decoder State
    /// <summary>
    /// Always 0.
    /// </summary>
    public ushort Gain; // 58

    /// <summary>
    /// Predictor Scale.
    /// </summary>
    public ushort PredictorScale; // 60

    public ushort SampleHistory1; // 62
    public ushort SampleHistory2; // 64

    // Loop Context
    public ushort LoopPredictorScale; // 66
    public ushort LoopSampleHistory1; // 68
    public ushort LoopSampleHistory2; // 70
    
    // Remainder
    public fixed short Reserved[PadShorts];

    public DspHeader(FileTableEntry entry) : this()
    {
        // Uwu
        NumSamples   = (uint) GcAdpcmMath.ByteCountToSampleCount(entry.Size);
        AdpcmNibbles = (uint) GcAdpcmMath.SampleCountToNibbleCount((int)NumSamples); // BUG: Some files specify 1 nibble too much. This is a crappy workaround.

        SampleRate     = entry.SampleRate;
        PredictorScale = (ushort)entry.MaybePredictorScale;
        CurrentAddress = (uint)entry.CurrentAddress;

        for (int x = 0; x < FileTableEntry.AdpcmCoefficientCount; x++)
            AdpcmCoefficients[x] = entry.AdpcmCoefficients[x];
    }

    /// <summary>
    /// Reads the struct contents from a given stream.
    /// </summary>
    /// <param name="reader">The stream to read the contents from.</param>
    public void Read(BigEndianStreamReader reader)
    {
        reader.Read(out NumSamples);
        reader.Read(out AdpcmNibbles);
        reader.Read(out SampleRate);

        reader.Read(out LoopFlag);
        reader.Read(out Format);
        reader.Read(out LoopStartOffset);
        reader.Read(out LoopEndOffset);
        reader.Read(out CurrentAddress);

        for (int x = 0; x < FileTableEntry.AdpcmCoefficientCount; x++)
            reader.Read(out AdpcmCoefficients[x]);

        reader.Read(out Gain);
        reader.Read(out PredictorScale);
        reader.Read(out SampleHistory1);
        reader.Read(out SampleHistory2);

        reader.Read(out LoopPredictorScale);
        reader.Read(out LoopSampleHistory1);
        reader.Read(out LoopSampleHistory2);

        for (int x = 0; x < PadShorts; x++)
            reader.Read(out Reserved[x]);
    }

    /// <summary>
    /// Writes the struct contents to a given stream.
    /// </summary>
    /// <param name="stream">The stream to append the struct to.</param>
    public void Write(Stream stream)
    {
        stream.WriteBigEndianPrimitive(NumSamples);
        stream.WriteBigEndianPrimitive(AdpcmNibbles);
        stream.WriteBigEndianPrimitive(SampleRate);

        stream.WriteBigEndianPrimitive(LoopFlag);
        stream.WriteBigEndianPrimitive(Format);
        stream.WriteBigEndianPrimitive(LoopStartOffset);
        stream.WriteBigEndianPrimitive(LoopEndOffset);
        stream.WriteBigEndianPrimitive(CurrentAddress);

        for (int x = 0; x < FileTableEntry.AdpcmCoefficientCount; x++)
            stream.WriteBigEndianPrimitive(AdpcmCoefficients[x]);

        stream.WriteBigEndianPrimitive(Gain);
        stream.WriteBigEndianPrimitive(PredictorScale);
        stream.WriteBigEndianPrimitive(SampleHistory1);
        stream.WriteBigEndianPrimitive(SampleHistory2);

        stream.WriteBigEndianPrimitive(LoopPredictorScale);
        stream.WriteBigEndianPrimitive(LoopSampleHistory1);
        stream.WriteBigEndianPrimitive(LoopSampleHistory2);

        for (int x = 0; x < PadShorts; x++)
            stream.WriteBigEndianPrimitive(Reserved[x]);
    }
}