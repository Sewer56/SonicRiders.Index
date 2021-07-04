using System.Collections.Generic;
using IndexTool.Misc;
using IndexTool.Structs.Interfaces;

namespace IndexTool.Structs
{
    public class InternalFileType : ITemplateStructure
    {
        public int? Id;
        public string Name;

        public List<InternalFormat> InternalFormats;
        public List<string> SeenAt;
        
        /// <summary>
        /// Copies all the details aside the extension from the target.
        /// </summary>
        /// <param name="type">The type.</param>
        public void CopyFrom(InternalFileType type)
        {
            if (type.Id.HasValue)
                Id = type.Id;
            
            Utilities.AssignIfNotNullOrEmpty(ref Name, type.Name);

            if (type.SeenAt != null)
                SeenAt = type.SeenAt;

            if (type.InternalFormats != null)
                InternalFormats = type.InternalFormats;
        }

        public class InternalFormat
        {
            public int Id;
            public string Name;
        }
    }
}
