using System;
using IndexTool.Misc;
using IndexTool.Options.Interfaces;

namespace IndexTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IndexTool by Sewer56");
            Console.WriteLine("Tool to automatically generate wiki data.");

            var options = Utilities.MakeAllInstances<IOption>();
            var optionPicker = new OptionPicker(options);
            while (true)
                optionPicker.Pick();
        }
    }
}
