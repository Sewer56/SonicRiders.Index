namespace GcaxDatInjector.Utilities;

internal interface IDspAudioSource
{
    /// <summary>
    /// Returns the audio as a DSP file.
    /// </summary>
    public byte[] GetData();
}