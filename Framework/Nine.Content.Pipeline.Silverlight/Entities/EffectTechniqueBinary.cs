using System.Collections.Generic;

namespace Nine.Content.Pipeline.Silverlight
{
    class EffectTechniqueBinary
    {
        public string Name { get; set; }
        public List<EffectPassBinary> PassBinaries { get; private set; } 

        public EffectTechniqueBinary()
        {
            PassBinaries = new List<EffectPassBinary>();
        }
    }
}
