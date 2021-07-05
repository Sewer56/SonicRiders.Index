using System.IO;
using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;

namespace IndexTool.Options.TypeGuesser
{
    public class AdxGuesser : IFileTypeGuesser
    {
        public string GetId() => "ADX";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();

            using var bigEndianReader = new BigEndianStreamReader(data);
            var pos = bigEndianReader.Position();
            
            // Magic Header
            if (bigEndianReader.Read<ushort>() != 0x8000)
                return false;

            // Copyright offset
            var copyrightOffset = bigEndianReader.Read<ushort>();
            bigEndianReader.Seek(pos + copyrightOffset, SeekOrigin.Begin);

            return bigEndianReader.Read<int>() == 0x29435249; // )CRI
        }
    }
}
