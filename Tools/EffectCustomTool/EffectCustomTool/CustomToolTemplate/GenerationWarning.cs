using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nine.Tools.EffectCustomTool
{
    public class GenerationWarning
    {
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Message { get; set; }
    }
}
