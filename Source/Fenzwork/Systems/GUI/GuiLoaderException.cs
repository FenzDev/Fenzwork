using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fenzwork.Systems.GUI
{
#nullable enable
    public class GuiLoaderException(string fileName, int line, int column, string? message, Exception? innerException) : Exception(message, innerException)
#nullable restore
    {
        public string FileName { get; set; } = fileName;
        public int Line { get; set; } = line;
        public int Column { get; set; } = column;

        public override string Message => $"Error loading {FileName} at line {Line} col {Column} : {base.Message}";
    }
}
