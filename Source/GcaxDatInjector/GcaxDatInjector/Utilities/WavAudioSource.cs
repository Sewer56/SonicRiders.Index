using VGAudio.Containers.Dsp;
using VGAudio.Containers.Wave;

namespace GcaxDatInjector.Utilities;

internal class WavAudioSource : IDspAudioSource
{
    private string _file;
    private byte[]? _bytes;

    public WavAudioSource(string file)
    {
        this._file = file;
    }

    public byte[] GetData()
    {
        _bytes ??= ConvertToDsp();
        return _bytes;
    }

    private byte[] ConvertToDsp()
    {
        using var wavStream = new FileStream(_file, FileMode.Open);
        
        var dspWriter = new DspWriter();
        var wavReader = new WaveReader();
        var audio = wavReader.Read(wavStream);

        using var memoryStream = new MemoryStream();
        dspWriter.WriteToStream(audio, memoryStream);
        return memoryStream.ToArray();
    }
}