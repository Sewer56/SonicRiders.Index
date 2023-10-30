using System;
using System.Linq;
using System.Runtime.InteropServices;
using Sewer56.SonicRiders.Parser.Archive.Structs.Managed;
using Standart.Hash.xxHash;

namespace RidersArchiveTool.Deduplication.Structures;

internal class GroupHashCollection : IEquatable<GroupHashCollection>
{
    /// <summary>
    /// Id of the individual group.
    /// </summary>
    public ushort Id;

    /// <summary>
    /// Combined hash code of all elements within the group.
    /// </summary>
    public ulong Hash;

    /// <summary>
    /// Hashes of all of the files contained within the group.
    /// </summary>
    public ulong[] Hashes;

    internal GroupHashCollection(in ManagedGroup group)
    {
        Id = group.Id;
        Hashes = new ulong[group.Files.Count];
        ulong combinedHash = 0;

        for (int x = 0; x < Hashes.Length; x++)
        {
            var data = group.Files[x].Data;
            if (data.Length == 0)
                Hashes[x] = 0;
            else
                Hashes[x] = xxHash64.ComputeHash(group.Files[x].Data);
            
            combinedHash = CombineHashes(combinedHash, Hashes[x]);
        }

        Hash = combinedHash;
    }

    private static ulong CombineHashes(ulong first, ulong second)
    {
        Span<ulong> hashes = stackalloc ulong[2];
        var asBytes = MemoryMarshal.Cast<ulong, byte>(hashes);
        hashes[0] = first;
        hashes[1] = second;
        return xxHash64.ComputeHash(asBytes, asBytes.Length);
    }

    // Auto Implemented
    public bool Equals(GroupHashCollection other)
    {
        return Hash == other.Hash && 
               Id == other.Id &&
               Hashes.SequenceEqual(other.Hashes);
    }

    public override bool Equals(object obj)
    {
        return obj is GroupHashCollection other && Equals(other);
    }

    public override int GetHashCode() => (int) Hash;
}