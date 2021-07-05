using System;
using System.IO;
using IndexTool.Options.TypeGuesser.Interfaces;
using Reloaded.Memory.Streams;
using Sewer56.SonicRiders.Parser.Portal.Structs;
using Sewer56.SonicRiders.Utility.Math;

namespace IndexTool.Options.TypeGuesser
{
    public class ObjectPortalGuesser : IFileTypeGuesser
    {
        public string GetId()
        {
            return "RIDERS-OBJPORTAL";
        }

        public bool TryGuess(BufferedStreamReader data, int streamLength, out string id)
        {
            id = GetId();

            // Read header.
            data.Read<byte>(out var numPortals);
            data.Read<byte>(out var magic);

            if (magic != 0x80)
                return false;

            // Min-max portal ASCII chars.
            data.Read<byte>(out var minPortal);
            data.Read<byte>(out var maxPortal);

            data.Seek(4, SeekOrigin.Current);
            for (int x = 0; x < numPortals; x++)
            {
                var portal = data.Read<Portal>();
                var min = portal.Minimum;
                var max = portal.Maximum;

                // Bounds checks, minimum should always be smaller than max.
                if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
                    return false;

                // Check for valid portal.
                if (portal.PortalChar > maxPortal || portal.PortalChar < minPortal)
                    return false;
            }

            return true;
        }
    }
}
