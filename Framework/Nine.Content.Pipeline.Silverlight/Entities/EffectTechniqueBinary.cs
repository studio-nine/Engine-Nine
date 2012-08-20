// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace SilverlightContentPipeline
{
    public class EffectTechniqueBinary
    {
        public string Name { get; set; }
        public List<EffectPassBinary> PassBinaries { get; private set; } 

        public EffectTechniqueBinary()
        {
            PassBinaries = new List<EffectPassBinary>();
        }
    }
}
