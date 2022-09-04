using System;
using IndexTool.Options.Interfaces;
using IndexTool.Structs.Attributes;

namespace IndexTool.Options.Helpers;

[ReflectionIgnore]
public class ActionOption : IOption
{
    public string Name;
    public Action Action;

    public ActionOption(string name, Action action)
    {
        Name = name;
        Action = action;
    }

    public ActionOption()
    {
    }

    public string GetName() => Name;
    public void Execute() => Action();
}