namespace Nine.Graphics.Materials
{
    using System.Linq;
    using System.Text;
    using Nine.Graphics.Materials;

    class MaterialPaintGroupBuilder
    {
        public string Build(MaterialPaintGroup materialPaintGroup, MaterialUsage usage)
        {
            var materialGroupBuilder = new MaterialGroupBuilder();
            var builder = new StringBuilder();
            var index = materialPaintGroup.MaterialGroup.MaterialParts.OfType<MaterialPaintGroup>().ToList().IndexOf(materialPaintGroup);
            var builderContext = materialGroupBuilder.CreateMaterialGroupBuilderContext(materialPaintGroup.MaterialParts, usage, false);
            
            builder.AppendLine(materialGroupBuilder.GetShaderCodeBody(builderContext, "VSMain", "PSMain"));

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

        private string AppendWithCasingCorrection(string name, string prefix)
        {
            return string.Concat(prefix, name.Substring(0, 1).ToUpperInvariant(), name.Substring(1));
        }
    }
}
