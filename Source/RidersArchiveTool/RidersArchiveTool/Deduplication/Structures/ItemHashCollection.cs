namespace RidersArchiveTool.Deduplication.Structures;

internal class ItemHashCollection
{
    public GroupHashCollection[] Hashes { get; }

    internal ItemHashCollection(in DeduplicatorItem item)
    {
        Hashes = new GroupHashCollection[item.Groups.Length];
        for (int x = 0; x < Hashes.Length; x++)
            Hashes[x] = new GroupHashCollection(item.Groups[x]);
    }
}