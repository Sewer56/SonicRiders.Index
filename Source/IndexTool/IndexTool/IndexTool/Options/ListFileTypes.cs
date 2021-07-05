using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IndexTool.Misc;
using IndexTool.Options.Interfaces;
using IndexTool.Services;
using IndexTool.Structs;
using IndexTool.Templates;

namespace IndexTool.Options
{
    public class ListFileTypes : IOption
    {
        public const string TemplateFolder = "ListFileTypes";
        public const string InternalTemplateFolder = "InternalFileTypes";

        public string GetName() => "List File Types";

        public void Execute()
        {
            Console.WriteLine("Note: You can override individual entries by modifying " + KnownTypesService.KnownTypesPath);
            string path    = Path.GetFullPath(Utilities.GetValidDirectory("Full Path to PC Riders' Data Folder"));
            var types         = GetTypes(path);
            var internalTypes = GetInternalTypes(path);

            Console.WriteLine("JSON [File Types]: ");
            Console.WriteLine(Utilities.ToJson(types));

            Console.WriteLine("JSON [Internal Types]: ");
            Console.WriteLine(Utilities.ToJson(internalTypes));

            Console.WriteLine("Template [File Types]: ");
            var generator  = new TemplateGenerator(TemplateFolder);
            Console.WriteLine(generator.Generate(new { types }));

            Console.WriteLine("Template [Internal Types]: ");
            generator = new TemplateGenerator(InternalTemplateFolder);
            Console.WriteLine(generator.Generate(new { types = internalTypes }));
        }

        public List<FileType> GetTypes(string ridersDataPath)
        {
            var files      = Directory.GetFiles(ridersDataPath, "*.*", SearchOption.AllDirectories);
            var types      = new List<FileType>();

            // Extract data.
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file).ToUpper();
                
                // Unique extension.
                if (types.FirstOrDefault(x => x.Extension == extension) != null)
                    continue;

                // Extract data.
                extension ??= "";
                types.Add(new FileType()
                {
                    Extension = extension,
                    Example   = file.Substring(ridersDataPath.Length + 1),
                });
            }

            // Inject known types.
            var knownTypes = KnownTypesService.GetKnownTypes();
            var typesDict  = types.ToDictionary(type => type.Extension);

            foreach (var knownType in knownTypes)
            {
                if (knownType.Extension != null && typesDict.TryGetValue(knownType.Extension, out var type))
                    type.CopyFrom(knownType);
            }

            return types;
        }

        /// <summary>
        /// Gets all file types that are stored inside Riders archives and never seen in standalone files.
        /// </summary>
        public List<FileType> GetInternalTypes(string ridersDataPath)
        {
            var types      = GetTypes(ridersDataPath);
            var knownTypes = KnownTypesService.GetKnownTypes();

            var typeIds = new HashSet<string>();
            foreach (var type in types)
                typeIds.Add(type.Id);

            return knownTypes.Where(x => !typeIds.Contains(x.Id)).ToList();
        }
    }
}
