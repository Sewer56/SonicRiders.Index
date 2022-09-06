namespace GcaxDatInjector.Utilities;

internal class DspAudioSource : IDspAudioSource
{
    private byte[] _bytes;

    public DspAudioSource(string file) => _bytes = File.ReadAllBytes(file);

    public DspAudioSource(byte[] bytes) => _bytes = bytes;

    public byte[] GetData() => _bytes;
}