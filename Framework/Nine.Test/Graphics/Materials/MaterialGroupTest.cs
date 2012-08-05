namespace Nine.Graphics.Materials.Test
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Nine.Content.Pipeline;
    using Nine.Content.Pipeline.Graphics.Materials;
    using Nine.Graphics.Materials.MaterialParts;

    [TestClass]
    public class MaterialGroupTest
    {
        [TestMethod()]
        [DeploymentItem("Nine.Test/Graphics/Materials/MaterialPart1.txt")]
        public void MaterialGroupLexerTest()
        {
            Lexer lexer = new Lexer(File.ReadAllText("MaterialPart1.txt"));
            lexer.Read();
        }

        [TestMethod()]
        public void MaterialGroupEmptyTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupDiffuseTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupSkinnedTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new SkinnedMaterialPart());
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupDiffuseEmissiveSpecularAndLightTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            materialGroup.MaterialParts.Add(new SpecularMaterialPart());
            materialGroup.MaterialParts.Add(new EmissiveMaterialPart());
            materialGroup.MaterialParts.Add(new DirectionalLightMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupNormalMappingTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            materialGroup.MaterialParts.Add(new SpecularMaterialPart());
            materialGroup.MaterialParts.Add(new NormalMapMaterialPart());
            materialGroup.MaterialParts.Add(new DirectionalLightMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupNormalMappingUsageTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            materialGroup.MaterialParts.Add(new SpecularMaterialPart());
            materialGroup.MaterialParts.Add(new NormalMapMaterialPart());
            materialGroup.MaterialParts.Add(new DirectionalLightMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.DepthAndNormal, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupSkinnedNormalMappingTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new SkinnedMaterialPart());
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            materialGroup.MaterialParts.Add(new SpecularMaterialPart());
            materialGroup.MaterialParts.Add(new NormalMapMaterialPart());
            materialGroup.MaterialParts.Add(new DirectionalLightMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupDeferredLightingTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            materialGroup.MaterialParts.Add(new NormalMapMaterialPart());
            materialGroup.MaterialParts.Add(new SpecularMaterialPart());
            materialGroup.MaterialParts.Add(new DeferredLightsMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialGroupDeferredGraphicsBufferTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            materialGroup.MaterialParts.Add(new DiffuseMaterialPart());
            materialGroup.MaterialParts.Add(new SpecularMaterialPart());
            materialGroup.MaterialParts.Add(new DeferredLightsMaterialPart());
            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.DepthAndNormal, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialPaintGroupTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            MaterialPaintGroup layer0 = new MaterialPaintGroup();
            MaterialPaintGroup layer1 = new MaterialPaintGroup();
            
            layer0.MaterialParts.Add(new DiffuseMaterialPart());
            layer1.MaterialParts.Add(new DiffuseMaterialPart());
            
            materialGroup.MaterialParts.Add(layer0);
            materialGroup.MaterialParts.Add(layer1);

            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialPaintGroupSpecularTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            MaterialPaintGroup layer0 = new MaterialPaintGroup();
            MaterialPaintGroup layer1 = new MaterialPaintGroup();

            layer0.MaterialParts.Add(new DiffuseMaterialPart());
            layer0.MaterialParts.Add(new SpecularMaterialPart());
            layer1.MaterialParts.Add(new DiffuseMaterialPart());

            materialGroup.MaterialParts.Add(layer0);
            materialGroup.MaterialParts.Add(layer1);
            materialGroup.MaterialParts.Add(new DirectionalLightMaterialPart());

            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialPaintGroupNormalMappingTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            MaterialPaintGroup layer0 = new MaterialPaintGroup();
            MaterialPaintGroup layer1 = new MaterialPaintGroup();

            layer0.MaterialParts.Add(new DiffuseMaterialPart());
            layer0.MaterialParts.Add(new SpecularMaterialPart());
            layer0.MaterialParts.Add(new NormalMapMaterialPart());
            layer1.MaterialParts.Add(new DiffuseMaterialPart());
            layer1.MaterialParts.Add(new NormalMapMaterialPart());

            materialGroup.MaterialParts.Add(layer0);
            materialGroup.MaterialParts.Add(layer1);
            materialGroup.MaterialParts.Add(new DirectionalLightMaterialPart());

            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialPaintGroupDeferredLightingTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            MaterialPaintGroup layer0 = new MaterialPaintGroup();
            MaterialPaintGroup layer1 = new MaterialPaintGroup();

            layer0.MaterialParts.Add(new DiffuseMaterialPart());
            layer0.MaterialParts.Add(new SpecularMaterialPart());
            layer0.MaterialParts.Add(new NormalMapMaterialPart());
            layer1.MaterialParts.Add(new DiffuseMaterialPart());
            layer1.MaterialParts.Add(new NormalMapMaterialPart());

            materialGroup.MaterialParts.Add(layer0);
            materialGroup.MaterialParts.Add(layer1);
            materialGroup.MaterialParts.Add(new DeferredLightsMaterialPart());

            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.Default, new PipelineBuilder().ProcessorContext);
        }

        [TestMethod()]
        public void MaterialPaintGroupDeferredLightingUsageTest()
        {
            MaterialGroup materialGroup = new MaterialGroup();
            MaterialPaintGroup layer0 = new MaterialPaintGroup();
            MaterialPaintGroup layer1 = new MaterialPaintGroup();

            layer0.MaterialParts.Add(new DiffuseMaterialPart());
            layer0.MaterialParts.Add(new SpecularMaterialPart());
            layer0.MaterialParts.Add(new NormalMapMaterialPart());
            layer1.MaterialParts.Add(new DiffuseMaterialPart());
            layer1.MaterialParts.Add(new NormalMapMaterialPart());

            materialGroup.MaterialParts.Add(layer0);
            materialGroup.MaterialParts.Add(layer1);
            materialGroup.MaterialParts.Add(new DeferredLightsMaterialPart());

            MaterialGroupBuilder.Build(materialGroup, MaterialUsage.DepthAndNormal, new PipelineBuilder().ProcessorContext);
        }
    }
}
