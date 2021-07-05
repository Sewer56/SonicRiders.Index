using System;
using IndexTool.Misc;
using IndexTool.Structs.Interfaces;

namespace IndexTool.Structs
{
    public class FileType : ITemplateStructure
    {
        public string Id;
        public string Extension;
        public string Format;
        public string Tool;
        public string ToolUrl;
        public string Template;
        public string TemplateUrl;
        public string Description;
        public string Example;

        /// <summary>
        /// Copies all the details aside the extension from the target.
        /// </summary>
        /// <param name="type">The type.</param>
        public void CopyFrom(FileType type)
        {
            Utilities.AssignIfNotNullOrEmpty(ref Id, type.Id);
            Utilities.AssignIfNotNullOrEmpty(ref Format, type.Format);
            Utilities.AssignIfNotNullOrEmpty(ref Tool, type.Tool);
            Utilities.AssignIfNotNullOrEmpty(ref ToolUrl, type.ToolUrl);
            Utilities.AssignIfNotNullOrEmpty(ref Template, type.Template);
            Utilities.AssignIfNotNullOrEmpty(ref TemplateUrl, type.TemplateUrl);
            Utilities.AssignIfNotNullOrEmpty(ref Description, type.Description);
            Utilities.AssignIfNotNullOrEmpty(ref Example, type.Example);
        }
    }
}
