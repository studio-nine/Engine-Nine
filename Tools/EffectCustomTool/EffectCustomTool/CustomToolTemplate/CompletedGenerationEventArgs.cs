using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace CustomToolTemplate
{
    public class CompletedGenerationEventArgs : EventArgs
    {
        public CompletedGenerationEventArgs(
            string generatedOutput,
            ProjectItem projectItem
            )
        {
            GeneratedOutput = generatedOutput;
            ProjectItem = projectItem;
        }

        public string GeneratedOutput { get; private set; }
        public ProjectItem ProjectItem { get; private set; }

    }
}
