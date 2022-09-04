using System;
using System.Collections.Generic;
using System.Text.Json;
using IndexTool.Misc;
using IndexTool.Options.Helpers;
using IndexTool.Options.Interfaces;
using IndexTool.Structs.Interfaces;

namespace IndexTool.Options;

public class GenerateJsonTemplate : IOption
{
    public string GetName()
    {
        return "Generate Json Template for Existing Known Data";
    }

    public void Execute()
    {
        var options = new List<ActionOption>();
        var types   = Utilities.GetAllInstantiableTypes<ITemplateStructure>();

        foreach (var type in types)
        {
            options.Add(new ActionOption(type.Name, () =>
            {
                var genericListType  = typeof(List<>);
                var concreteListType = genericListType.MakeGenericType(type);

                var array = Array.CreateInstance(type, 1);
                array.SetValue(Activator.CreateInstance(type), 0);

                var list  = Activator.CreateInstance(concreteListType, new object[] { array });
                Console.WriteLine(JsonSerializer.Serialize(list, Utilities.JsonSerializerOptions));
            }));
        }

        var optionPicker = new OptionPicker(options);
        optionPicker.Pick("Select a Structure");
    }
}