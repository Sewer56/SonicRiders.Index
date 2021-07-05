using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser
{
    public class AixGuesser : IFileTypeGuesser
    {
        public string GetId() => "AIX";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();
            return data.Read<int>() == 0x46584941; // 'AIXF'
        }
    }
}
