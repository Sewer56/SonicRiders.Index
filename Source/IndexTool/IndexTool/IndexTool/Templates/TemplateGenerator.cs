using System.Collections.Generic;
using System.IO;
using IndexTool.Options.Helpers;
using Scriban;
using Scriban.Runtime;

namespace IndexTool.Templates;

/// <summary>
/// Provides tools for querying available templates
/// </summary>
public class TemplateGenerator
{
    /// <summary>
    /// Full path of the folder containing templates.
    /// </summary>
    public string Directory { get; private set; }

    static TemplateGenerator()
    {
    }
        
    /// <param name="directory">Name of the directory inside the `Templates` folder.</param>
    public TemplateGenerator(string directory)
    {
        Directory = $"{Path.GetDirectoryName(typeof(TemplateGenerator).Assembly.Location)}/Templates/{directory}";
    }

    /// <summary>
    /// Picks a template and generates it.
    /// </summary>
    public string Generate(object scribanModel)
    {
        var templates = System.IO.Directory.GetFiles(Directory, "*.txt");
        string templateFile = null;

        // Pick template file if more than 1 available.
        if (templates.Length > 1)
        {
            var options = new List<ActionOption>();
            foreach (var tmp in templates)
                options.Add(new ActionOption(Path.GetFileName(tmp), () => templateFile = tmp));

            var optionPicker = new OptionPicker(options);
            optionPicker.Pick("Select a Template");
        }
        else
        {
            templateFile = templates[0];
        }

        // Render template
        var scriptobj = new ScriptObject();
        scriptobj.Import(scribanModel);

        var context = new TemplateContext();
        context.LoopLimit = 0;
        context.PushGlobal(scriptobj);

        var template = Template.Parse(File.ReadAllText(templateFile));
        return template.Render(context);
    }
}