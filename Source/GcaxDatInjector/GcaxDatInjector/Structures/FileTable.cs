using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;

namespace GcaxDatInjector.Structures;

public struct FileTable
{
    /// <summary>
    /// Number of files present, minus one.
    /// </summary>
    public int FileCountMinusOne;

    /// <summary>
    /// All entries in this table.
    /// </summary>
    public FileTableEntry[] Entries;

    /// <summary>
    /// Reads the file table from a given stream.
    /// </summary>
    /// <param name="reader">The reader to read table from.</param>
    public static FileTable Read(EndianStreamReader reader)
    {
        var result = new FileTable();
        result.FileCountMinusOne = reader.Read<int>();
        result.Entries = GC.AllocateUninitializedArray<FileTableEntry>(result.FileCountMinusOne + 1);

        for (int x = 0; x < result.FileCountMinusOne + 1; x++)
            result.Entries[x] = FileTableEntry.Read(reader);

        return result;
    }

    /// <summary>
    /// Writes the file table to a given stream.
    /// </summary>
    /// <param name="output"></param>
    public void Write(Stream output)
    {
        output.WriteBigEndianPrimitive(FileCountMinusOne);
        for (int x = 0; x < FileCountMinusOne + 1; x++)
            Entries[x].Write(output);
    }
}