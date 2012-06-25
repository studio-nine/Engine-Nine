#region File Description
//-----------------------------------------------------------------------------
// ContentBuilder.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nine.Content.Pipeline.Graphics.Materials;
using Nine.Graphics.Materials.MaterialParts;
using Nine.Content.Pipeline;

#endregion

namespace Nine.Graphics.Materials.Test
{
    [TestClass]
    public class MaterialGroupTest
    {
        [TestMethod()]
        //[DeploymentItem("Nine.Test/Graphics/Materials/MaterialPart1.txt")]
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
    }
}
