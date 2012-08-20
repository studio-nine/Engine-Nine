// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace SilverlightContentPipeline
{
    public class EffectBinary
    {
        public List<EffectTechniqueBinary> TechniquesBinaries { get; private set; }

        public EffectBinary()
        {
            TechniquesBinaries = new List<EffectTechniqueBinary>();
        }
    }
}
