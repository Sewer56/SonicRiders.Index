using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IndexTool.Misc;
using IndexTool.Options.Interfaces;
using IndexTool.Structs;
using IndexTool.Templates;

namespace IndexTool.Options
{
    public class ListFileTypes : IOption
    {
        public const string TemplateFolder           = "ListFileTypes";
        public static readonly string KnownTypesPath = $"{Path.GetDirectoryName(typeof(TemplateGenerator).Assembly.Location)}/Assets/KnownFileTypes.json";

        public string GetName() => "List File Types";

        public void Execute()
        {
            Console.WriteLine("Note: You can override individual entries by modifying " + KnownTypesPath);
            string path    = Path.GetFullPath(Utilities.GetValidDirectory("Full Path to PC Riders' Data Folder"));
            var types      = GetTypes(path);

            Console.WriteLine("JSON: ");
            Console.WriteLine(Utilities.ToJson(types));

            Console.WriteLine("Template: ");
            var generator  = new TemplateGenerator(TemplateFolder);
            Console.WriteLine(generator.Generate(new { types }));
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
                    Example   = file.Substring(ridersDataPath.Length + 1)
                });
            }

            // Inject known types.
            var knownTypes = Utilities.FromJsonOrEmpty<List<FileType>>(KnownTypesPath);
            var typesDict  = types.ToDictionary(type => type.Extension);

            foreach (var knownType in knownTypes)
            {
                if (knownType.Extension != null && typesDict.TryGetValue(knownType.Extension, out var type))
                    type.CopyFrom(knownType);
            }

            return types;
        }
    }
}
