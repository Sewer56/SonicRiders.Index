using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser
{
    public class DtpkGuesser : IFileTypeGuesser
    {
        public string GetId() => "DTPK";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();
            return data.Peek<int>() == 0x78626F78; // 'xbox'
        }
    }
}
