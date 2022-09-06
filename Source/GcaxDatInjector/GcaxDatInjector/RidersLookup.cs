using System.Text;

namespace GcaxDatInjector;

internal static class RidersLookup
{
    internal static RidersFileEntry[] KnownFiles = new RidersFileEntry[]
    {
        new("0501.DAT", 0x805E58E4),
        new("0502.DAT", 0x805E5900),
        new("0503.DAT", 0x805E591C),
        new("0504.DAT", 0x805E5938),
        new("0505.DAT", 0x805E5954),
        new("0506.DAT", 0x805E5970),
        new("0507.DAT", 0x805E598C),
        new("0508.DAT", 0x805E59A8),
        new("0509.DAT", 0x805E59C4),

        new("10JSONIC.DAT", 0x805E59E0),
        new("10JTAILS.DAT", 0x805E59FC),
        new("10JKNUCK.DAT", 0x805E5A18),
        new("10JAMY.DAT", 0x805E5A34),
        new("10JJET.DAT", 0x805E5A50),
        new("10JSTORM.DAT", 0x805E5A6C),
        new("10JWAVE.DAT", 0x805E5A88),
        new("10JEGGM.DAT", 0x805E5AA4),
        new("10JCREAM.DAT", 0x805E5AC0),
        new("10JROUGE.DAT", 0x805E5ADC),
        new("10JSHADO.DAT", 0x805E5AF8),
        new("10JSS.DAT", 0x805E5B14),
        new("10JAIAI.DAT", 0x805E5B4C),
        new("10JULALA.DAT", 0x805E5B68),
        new("10JER1.DAT", 0x805E5B84),
        new("10JER2.DAT", 0x805E5BA0),

        new("10SONIC.DAT", 0x805E5BBC),
        new("10TAILS.DAT", 0x805E5BD8),
        new("10KNUCK.DAT", 0x805E5BF4),
        new("10AMY.DAT", 0x805E5C10),
        new("10JET.DAT", 0x805E5C2C),
        new("10STORM.DAT", 0x805E5C48),
        new("10WAVE.DAT", 0x805E5C64),
        new("10EGGM.DAT", 0x805E5C80),
        new("10CREAM.DAT", 0x805E5C9C),
        new("10ROUGE.DAT", 0x805E5CB8),
        new("10SHADO.DAT", 0x805E5CD4),
        new("10SS.DAT", 0x805E5CF0),
        new("10AIAI.DAT", 0x805E5D28),
        new("10ULALA.DAT", 0x805E5D44),
        new("10ER1.DAT", 0x805E5D60),
        new("10ER2.DAT", 0x805E5D7C)
    };

    public static bool TryGetGeckoCode(string fileName, int endOfTbld, int pcmdSectionSize, out string? geckoTbld)
    {
        var entry = KnownFiles.FirstOrDefault(x => x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (entry.MemoryAddress != default)
        {
            const uint gcnBaseAddr = 0x80000000;
            var pEndOfTbld = (entry.MemoryAddress - gcnBaseAddr) + 0x10;
            var pPcmdSectionSize = pEndOfTbld + 4;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"04{pEndOfTbld:X} {endOfTbld:X8}");
            builder.AppendLine($"04{pPcmdSectionSize:X} {pcmdSectionSize:X8}");
            geckoTbld = builder.ToString();
            return true;
        }

        geckoTbld = null;
        return false;
    }
}

public struct RidersFileEntry
{
    public string FileName   ;
    public uint MemoryAddress;

    public RidersFileEntry(string fileName, uint memoryAddress)
    {
        FileName = fileName;
        MemoryAddress = memoryAddress;
    }
}