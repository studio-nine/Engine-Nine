using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomToolTemplate
{
    public class GenerationError
    {
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Message { get; set; }
    }
}
