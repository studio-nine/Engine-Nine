// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;

namespace Nine.Content.Pipeline.Silverlight
{
    public class EffectPassBinary
    {
        public string Name { get; set; }
        public byte[] VertexShaderByteCode { get; set; }
        public byte[] VertexShaderParameters { get; set; }
        public byte[] PixelShaderByteCode { get; set; }
        public byte[] PixelShaderParameters { get; set; }
        public Dictionary<string, string> RenderStates { get; set; }
    }
}
