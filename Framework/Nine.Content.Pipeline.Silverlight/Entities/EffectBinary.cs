// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.IO;

namespace Nine.Content.Pipeline.Silverlight
{
    public class EffectBinary
    {
        [Microsoft.Xna.Framework.Content.ContentSerializer]
        public List<EffectTechniqueBinary> TechniquesBinaries { get; private set; }

        public EffectBinary()
        {
            TechniquesBinaries = new List<EffectTechniqueBinary>();
        }

        public byte[] GetEffectCode()
        {
            var stream = new MemoryStream();
            using (var output = new BinaryWriter(stream))
            {
                // Browsing techniques
                output.Write(TechniquesBinaries.Count);
                foreach (var techniqueBinary in TechniquesBinaries)
                {
                    // Browsing passes
                    output.Write(techniqueBinary.PassBinaries.Count);
                    foreach (var passBinary in techniqueBinary.PassBinaries)
                    {
                        output.Write(passBinary.Name);

                        // Vertex shader
                        if (passBinary.VertexShaderByteCode == null)
                        {
                            output.Write(0);
                        }
                        else
                        {
                            output.Write(passBinary.VertexShaderByteCode.Length);
                            output.Write(passBinary.VertexShaderByteCode);
                            output.Write(passBinary.VertexShaderParameters.Length);
                            output.Write(passBinary.VertexShaderParameters);
                        }

                        // Pixel shader
                        if (passBinary.PixelShaderByteCode == null)
                        {
                            output.Write(0);
                        }
                        else
                        {
                            output.Write(passBinary.PixelShaderByteCode.Length);
                            output.Write(passBinary.PixelShaderByteCode);
                            output.Write(passBinary.PixelShaderParameters.Length);
                            output.Write(passBinary.PixelShaderParameters);
                        }

                        // Render states
                        output.Write(passBinary.RenderStates.Count);
                        foreach (string key in passBinary.RenderStates.Keys)
                        {
                            output.Write(key);
                            output.Write(passBinary.RenderStates[key]);
                        }
                    }
                }
            }
            return stream.ToArray();
        }
    }
}
