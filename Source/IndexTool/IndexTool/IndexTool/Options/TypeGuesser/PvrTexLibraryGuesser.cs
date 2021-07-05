using System;
using System.IO;
using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser
{
    public class PvrTexLibraryGuesser : IFileTypeGuesser
    {
        public string GetId() => "PVR-XVRS";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();
            var initialPos = data.Position();

            // Check file count.
            data.Read<short>(out var numTextures);
            if (numTextures < 1)
                return false;

            // Check optional section flag.
            data.Read<short>(out var hasOptionalSection);
            if (hasOptionalSection is not (0 or 256))
                return false;

            // Check if offsets are sequential.
            for (int x = 0; x < numTextures; x++)
            {
                // Get next offset.
                data.Read<int>(out var offset);
                var nextOffset = data.Position();

                // Seek to texture header.
                data.Seek(initialPos + offset, SeekOrigin.Begin);
                if (data.Peek<int>() != 0x58494247) // 'GBIX'
                    return false;

                data.Seek(nextOffset, SeekOrigin.Begin);
            }

            return true;
        }
    }
}
