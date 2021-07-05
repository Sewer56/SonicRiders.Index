using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;

namespace IndexTool.Options.TypeGuesser
{
    public class SfdGuesser : IFileTypeGuesser
    {
        public string GetId() => "SFD";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();

            // This is actually a modified MPEG container.
            // https://en.wikipedia.org/wiki/MPEG_program_stream
            using var endianStreamReader = new BigEndianStreamReader(data);
            if (endianStreamReader.Read<int>() != 0x1BA)
                return false;

            byte mpegType = endianStreamReader.Read<byte>();
            return mpegType >> 4 == 0b0010; // Check MPEG 1 variant.
        }
    }
}
