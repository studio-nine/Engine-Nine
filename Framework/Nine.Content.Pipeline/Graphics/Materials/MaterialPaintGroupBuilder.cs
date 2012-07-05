#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Text;
using Nine.Graphics.Materials;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Collections;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Text.RegularExpressions;
using Nine.Graphics.Materials.MaterialParts;
using System.Security.Cryptography;
#endregion

namespace Nine.Content.Pipeline.Graphics.Materials
{
    static class MaterialPaintGroupBuilder
    {
        public static string Build(MaterialPaintGroup materialPaintGroup, MaterialUsage usage)
        {
            var builder = new StringBuilder();
            var index = materialPaintGroup.MaterialGroup.MaterialParts.OfType<MaterialPaintGroup>().ToList().IndexOf(materialPaintGroup);
            var builderContext = MaterialGroupBuilder.CreateMaterialGroupBuilderContext(materialPaintGroup.MaterialParts, usage, false);
            
            builder.AppendLine(MaterialGroupBuilder.GetShaderCodeBody(builderContext, "VSMain", "PSMain"));

            builderContext.PixelShaderOutputs.AddRange(builderContext.PixelShaderInputs.Where(psi => psi.Out));

            builder.Append("void PixelShader(");
            builder.Append(string.Join(", ", Enumerable.Range(0, 1).Select(i => string.Concat("float paintBlend", index))
                .Concat(builderContext.PixelShaderInputs.Select(psi => string.Concat(psi.Type, " ", psi.Name)))
                .Concat(builderContext.PixelShaderOutputs.Select(pso => string.Concat("inout ", pso.Type, " ", AppendWithCasingCorrection(pso.Name, "paint"))))));
            builder.AppendLine(")");
            builder.AppendLine("{");
            foreach (var psi in builderContext.PixelShaderInputs)
                builder.AppendLine(string.Concat("    ", psi.Type, " _", psi.Name, " = ", psi.Name, ";"));
            foreach (var pso in builderContext.PixelShaderOutputs.Where(pso => !pso.In))
                builder.AppendLine(string.Concat("    ", pso.Type, " _", pso.Name, ";"));
            builder.Append("    PSMain(");
            builder.Append(string.Join(", ", builderContext.PixelShaderInputs.Select(psi => psi.Out ? string.Concat("_", psi.Name)  : psi.Name)
                                      .Concat(builderContext.PixelShaderOutputs.Where(pso => !pso.In).Select(pso => string.Concat("_", pso.Name)))));
            builder.AppendLine(");");
            foreach (var pso in builderContext.PixelShaderOutputs)
                builder.AppendLine(string.Concat("    ", AppendWithCasingCorrection(pso.Name, "paint"), " += _", pso.Name, " * paintBlend", index, ";"));
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static string AppendWithCasingCorrection(string name, string prefix)
        {
            return string.Concat(prefix, name.Substring(0, 1).ToUpperInvariant(), name.Substring(1));
        }
    }
}
