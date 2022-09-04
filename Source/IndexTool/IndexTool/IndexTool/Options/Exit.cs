using System;
using IndexTool.Options.Interfaces;

namespace IndexTool.Options;

public class Exit : IOption
{
    public string GetName() => "Exit the Program";
    public void Execute()
    {
        Environment.Exit(0);
    }
}