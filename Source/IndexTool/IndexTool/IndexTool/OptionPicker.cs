using System;
using System.Collections.Generic;
using IndexTool.Options.Interfaces;

namespace IndexTool;

/// <summary>
/// Utility class for selecting options from a list.
/// </summary>
public class OptionPicker
{
    public IReadOnlyList<IOption> Options { get; private set; }

    public OptionPicker(IReadOnlyList<IOption> options)
    {
        Options = options;
    }

    public void Pick(string label = "Select an Option")
    {
        RenderOptions(label);
        while (true)
        {
            var key = Console.ReadLine();
            if (!int.TryParse(key, out var result))
            {
                Console.WriteLine($"{key} is not a number");
                continue;
            }

            if (result < 0 || result > Options.Count - 1)
            {
                Console.WriteLine($"Number is outside of range (0 - {Options.Count - 1})");
                continue;
            }

            Options[result].Execute();
            return;
        }
    }

    private void RenderOptions(string label)
    {
        Console.WriteLine($"{label}: ");
        for (var x = 0; x < Options.Count; x++)
            Console.WriteLine($"{x}. {Options[x].GetName()}");

        Console.WriteLine("Type number associated with option and press enter.");
    }
}