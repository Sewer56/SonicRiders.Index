using System.Collections.Generic;

namespace RidersArchiveTool.Deduplication.Structures;

internal class ArchiveHashCollection
{
    internal Dictionary<int, List<ItemHashCollection>> HashToCollection { get; private set; } = new();

}