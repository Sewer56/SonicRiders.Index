using System;

namespace IndexTool.Options.Interfaces
{
    public interface IOption
    {
        public string GetName();
        public void Execute();
    }
}