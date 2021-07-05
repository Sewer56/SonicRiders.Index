using System.IO;
using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser
{
    public class XnfGuesser : IFileTypeGuesser
    {
        public string GetId() => "NN-XNF";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();
            if (data.Peek<int>() != 0x4649584E) // 'NXIF'
                return false;
            
            data.Seek(32, SeekOrigin.Current);
            return data.Peek<uint>() == 0x4D4D584E; // 'NXMM'
        }
    }
}
