using System.Collections.Generic;
using IndexTool.Misc;
using IndexTool.Structs.Interfaces;

namespace IndexTool.Structs;

public class InternalFileType : ITemplateStructure
{
    public int? Id;
    public string Name;

    public List<InternalFormat> InternalFormats;
    public List<string> SeenAt;

    public class InternalFormat
    {
        public int Id;
        public string Name;
    }
}