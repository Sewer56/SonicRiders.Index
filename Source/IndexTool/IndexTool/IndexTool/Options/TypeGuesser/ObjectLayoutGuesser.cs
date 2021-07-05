using System;
using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;

namespace IndexTool.Options.TypeGuesser
{
    public class ObjectLayoutGuesser : IFileTypeGuesser
    {
        public string GetId() => "RIDERS-OBJLAYOUT";

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();

            data.Read<short>(out var objectCount);
            data.Read<ushort>(out var magic);

            if (magic != 0x8000)
                return false;

            // Check size after header minus last section.
            return data.Read<int>() == (objectCount * 46) + 8;
        }
    }
}
