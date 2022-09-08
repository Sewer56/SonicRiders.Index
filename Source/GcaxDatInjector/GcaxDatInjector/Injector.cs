using System.Runtime.InteropServices.ComTypes;
using GcaxDatInjector.Structures;
using GcaxDatInjector.Utilities;
using RegExtract;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Streams.Readers;
using VGAudio.Codecs.Atrac9;
using VGAudio.Codecs.GcAdpcm;
using VGAudio.Containers.Dsp;
using VGAudio.Containers.Wave;

namespace GcaxDatInjector;

internal class Injector
{
    private const string ExtensionDsp = ".dsp";
    private const string ExtensionWav = ".wav";
    private const int FileTablePtr = 0xB8;

    /// <summary>
    /// Extracts sound files from an archive.
    /// </summary>
    internal static void Extract(ExtractOptions options)
    {
        using var datStream = new MemoryStream(File.ReadAllBytes(options.Source));
        using var streamReader  = new BufferedStreamReader(datStream, 2048);
        using var endianReader  = new BigEndianStreamReader(streamReader);
        var convert = options.Convert.GetValueOrDefault();

        Directory.CreateDirectory(options.Destination);

        // Get PCMD Section Ptr
        var pcmdSectionPtr = GetPcmdSectionPtr(endianReader);

        // Get File Table
        var table = GetFileTable(endianReader, out _);

        // Extract all files.
        for (var x = 0; x < table.Entries.Length; x++)
        {
            var entry = table.Entries[x];
            var filePath = Path.Combine(options.Destination, FormatStringForExport(x, convert));

            using var dspStream = GetDspOutputStream(filePath, convert);
            ConvertToDsp(dspStream, entry, endianReader);

            // Convert if necessary 
            if (convert)
            {
                using var wavStream = GetDspOutputStream(filePath, false);
                dspStream.Seek(0, SeekOrigin.Begin);

                var wavWriter = new WaveWriter();
                var dspReader = new DspReader();
                var audio = dspReader.Read(dspStream);
                
                wavWriter.WriteToStream(audio, wavStream);
            }
        }

        // Utility Functions
        string FormatStringForExport(int index, bool useWav)
        {
            var extension = useWav ? ExtensionWav : ExtensionDsp;
            return $"{index:00000}_Sound{extension}";
        }

        Stream GetDspOutputStream(string filePath, bool useWav)
        {
            if (!useWav)
                return new FileStream(filePath, FileMode.Create);

            return new MemoryStream();
        }
    }

    /// <summary>
    /// Injects new sound files into an archive.
    /// </summary>
    internal static unsafe void Inject(InjectOptions options)
    {
        const int PcmdHeaderSize = 32;

        // Get stream for original file.
        using var datStream = new MemoryStream(File.ReadAllBytes(options.Destination));
        using var streamReader = new BufferedStreamReader(datStream, 2048);
        using var endianReader = new BigEndianStreamReader(streamReader);

        // Where the newly created file will be stored.
        using var outputStream = new MemoryStream(datStream.Capacity * 2);

        // Get PCMD Section Ptr
        var pcmdSectionPtr = GetPcmdSectionPtr(endianReader);

        // Get File Table
        var table = GetFileTable(endianReader, out int fileTablePtr);

        // Copy everything up to PCMD Section.
        outputStream.Write(endianReader.ReadBytes(0, pcmdSectionPtr + PcmdHeaderSize));

        // Create DSP Audio Sources
        var files   = Directory.GetFiles(options.Source, "*.*", SearchOption.TopDirectoryOnly);
        var sources = new IDspAudioSource?[table.Entries.Length];

        foreach (var file in files)
        {
            var format = GetFormatFromExtension(file);
            if (format == ExtensionFormat.None)
                continue;

            var fileNameNoExtension = Path.GetFileNameWithoutExtension(file);
            int? index = null;

            try
            {
                index = fileNameNoExtension.Extract<int?>(@"^(\d+)");
                if (!index.HasValue)
                    continue;
            }
            catch (Exception e)
            {
                continue;
            }

            if (index.Value >= sources.Length)
            {
                Console.WriteLine($"Index for file: {file} goes beyond number of files in DAT.");
                continue;
            }

            sources[index.Value] = format switch
            {
                ExtensionFormat.Wav => new WavAudioSource(file),
                ExtensionFormat.DSP => new DspAudioSource(file),
                _ => throw new Exception("Unsupported Type")
            };
        }

        // Add missing audio sources.
        for (int x = 0; x < sources.Length; x++)
        {
            if (sources[x] != null)
                continue;

            var entry = table.Entries[x];
            using var dspStream = new MemoryStream(entry.Size + sizeof(DspHeader) + 1); // 96 = DSP Header Size
            ConvertToDsp(dspStream, entry, endianReader);

            sources[x] = new DspAudioSource(dspStream.ToArray());
        }

        // Rebuild audio sources.
        var currentPcmdOffset = PcmdHeaderSize; // Offset of first file.
        for (int x = 0; x < sources.Length; x++)
        {
            var bytes = sources[x]!.GetData();
            ref var entry = ref table.Entries[x];
            
            using var datMemoryStream = new MemoryStream(bytes);
            using var datStreamReader = new BufferedStreamReader(datMemoryStream, 2048);
            using var datEndianStreamReader = new BigEndianStreamReader(datStreamReader);

            var dspHeader = new DspHeader();
            dspHeader.Read(datEndianStreamReader);
            entry.FromDspHeader(dspHeader);
            entry.PcmdOffset = currentPcmdOffset;

            // Calculate remaining size.
            var size = (int)(datMemoryStream.Length - datStreamReader.Position());
            entry.Size = size;

            // Append to output & pad stream.
            outputStream.Write(datStreamReader.ReadBytes(datStreamReader.Position(), size));

            var streamPos = outputStream.Position;
            outputStream.AddPadding(GcAdpcmMath.BytesPerFrame);
            outputStream.AddPadding(8);
            var bytesPadded = outputStream.Position - streamPos;

            currentPcmdOffset += (size + (int)bytesPadded);
        }

        // Pad PCMD Section.
        outputStream.AddPadding(32);

        // Write updated table.
        outputStream.Seek(fileTablePtr, SeekOrigin.Begin);
        table.Write(outputStream);

        // Set PCMD Section Size
        var pcmdSectionSize = (int)outputStream.Length - pcmdSectionPtr;
        outputStream.Seek(0x18, SeekOrigin.Begin);
        outputStream.WriteBigEndianPrimitive(pcmdSectionSize);

        // Set PCMD Section Size Header
        const int pcmdHeaderSize = 32;
        outputStream.Seek(pcmdSectionPtr + 0x0C, SeekOrigin.Begin);
        outputStream.WriteBigEndianPrimitive(pcmdSectionSize - pcmdHeaderSize);

        // Pad File
        outputStream.Seek(0, SeekOrigin.End);
        outputStream.AddPadding(2048);

        // Set file size in output.
        outputStream.Seek(0xC, SeekOrigin.Begin);
        outputStream.WriteBigEndianPrimitive((int)outputStream.Length);

        File.WriteAllBytes(options.Destination, outputStream.ToArray());

        // Check
        if (options.PrintGecko)
        {
            if (RidersLookup.TryGetGeckoCode(Path.GetFileName(options.Destination), GetEndOfTbldPtr(endianReader), pcmdSectionSize, out string? gecko))
                Console.WriteLine($"Needed gecko code:\n{gecko}\n");
            else
                Console.WriteLine("No Gecko Code. Unrecognised file name.");
        }
    }

    private static void ConvertToDsp(Stream outputStream, FileTableEntry entry, BigEndianStreamReader datStreamReader)
    {
        var pcmdSectionPtr = GetPcmdSectionPtr(datStreamReader);
        var dspHeader = new DspHeader(entry);

        var offset = pcmdSectionPtr + entry.PcmdOffset;
        dspHeader.Write(outputStream);
        outputStream.Write(datStreamReader.ReadBytes(offset, entry.Size));
    }

    private static int GetPcmdSectionPtr(BigEndianStreamReader endianReader)
    {
        endianReader.Seek(0x1C, SeekOrigin.Begin);
        endianReader.Read(out int pcmdSectionPtr);
        return pcmdSectionPtr;
    }

    private static int GetEndOfTbldPtr(BigEndianStreamReader endianReader)
    {
        endianReader.Seek(0x10, SeekOrigin.Begin);
        endianReader.Read(out int tbldPointer);
        return tbldPointer;
    }

    private static int GetFileTablePtr(BigEndianStreamReader endianReader)
    {
        // Get File Table Ptr
        endianReader.Seek(0xB8, SeekOrigin.Begin);
        endianReader.Read(out int fileTablePtr);
        return fileTablePtr;
    }

    private static FileTable GetFileTable(BigEndianStreamReader endianReader, out int fileTablePtr)
    {
        fileTablePtr = GetFileTablePtr(endianReader);        
        endianReader.Seek(fileTablePtr, SeekOrigin.Begin);
        var table = FileTable.Read(endianReader);
        return table;
    }

    private static ExtensionFormat GetFormatFromExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        if (extension.Equals(ExtensionDsp, StringComparison.OrdinalIgnoreCase))
            return ExtensionFormat.DSP;
        
        if (extension.Equals(ExtensionWav, StringComparison.OrdinalIgnoreCase))
            return ExtensionFormat.Wav;

        return ExtensionFormat.None;
    }

    private enum ExtensionFormat
    {
        None,
        Wav,
        DSP
    }
}