using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser
{
    public class AfsGuesser : IFileTypeGuesser
    {
        public string GetId() => "AFS";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();
            return data.Read<int>() == 0x534641; // 'AFS'
        }
    }
}
