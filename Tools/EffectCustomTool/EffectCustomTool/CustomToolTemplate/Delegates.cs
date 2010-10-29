using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomToolTemplate
{
    /// <summary>
    /// The delegate in which to perform code Generation ofr the custom tool
    /// </summary>
    /// <param name="sender">will always be the custom tool implementation
    /// derived from CustomToolBase</param>
    /// <param name="args">The GenerationEventArgs used to write the generated 
    /// code to, or perform other generation-time tasks</param>
    public delegate void GenerationHandler(object sender, GenerationEventArgs args);

    /// <summary>
    /// The delegate to receive notification when other code has completed
    /// code generation.  Currently this is used to support testing, but may be
    /// useable in other contexts.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void CompletedGenerationHandler(object sender, CompletedGenerationEventArgs args);
}
