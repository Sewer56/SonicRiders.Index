using Sewer56.SonicRiders.Parser.Archive.Structs.Managed;

namespace RidersArchiveTool.Deduplication.Structures;

internal class DeduplicatorItem
{
    /// <summary>
    /// Absolute path to the file.
    /// </summary>
    public string FilePath { get; set; }
    
    /// <summary>
    /// The groups bound to the file.
    /// </summary>
    public ManagedGroup[] Groups { get; set; }
}