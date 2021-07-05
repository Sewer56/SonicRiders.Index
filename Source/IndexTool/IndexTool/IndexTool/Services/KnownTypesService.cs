using System;
using System.Collections.Generic;
using System.IO;
using IndexTool.Misc;
using IndexTool.Structs;
using IndexTool.Templates;

namespace IndexTool.Services
{
    public static class KnownTypesService
    {
        public static readonly string KnownTypesPath = $"{Path.GetDirectoryName(typeof(TemplateGenerator).Assembly.Location)}/Assets/KnownFileTypes.json";

        public static List<FileType> GetKnownTypes() => Utilities.FromJsonOrEmpty<List<FileType>>(KnownTypesPath);
        public static FileType FromId(string id)
        {
            var types = GetKnownTypes();
            foreach (var type in types)
            {
                if (type.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return type;
            }

            return new FileType() { Id = id };
        }
    }
}
