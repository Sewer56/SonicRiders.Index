using System.IO;
using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser
{
    public class XnoGuesser : IFileTypeGuesser
    {
        public string GetId() => "NN-XNO";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();
            if (data.Peek<int>() != 0x4649584E) // 'NXIF'
                return false;

            data.Seek(32, SeekOrigin.Current);

            // TODO: Texture library comes first???
            return data.Peek<uint>() == 0x4C54584E; // 'NXTL' 
        }
    }
}
