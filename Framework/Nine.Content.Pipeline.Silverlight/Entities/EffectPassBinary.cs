using System.Collections.Generic;

namespace Nine.Content.Pipeline.Silverlight
{
    class EffectPassBinary
    {
        public string Name { get; set; }
        public byte[] VertexShaderByteCode { get; set; }
        public byte[] VertexShaderParameters { get; set; }
        public byte[] PixelShaderByteCode { get; set; }
        public byte[] PixelShaderParameters { get; set; }
        public Dictionary<string, string> RenderStates { get; set; }
    }
}
