﻿#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Isles.Graphics.Models;
using Isles.Graphics;
#endregion


namespace Isles.Pipeline.Processors
{
    /// <summary>
    /// A custom processor to convert a mesh into a geometry.
    /// </summary>
    [ContentProcessor(DisplayName = "Geometry Processor - Isles")]
    public class GeometryProcessor : ContentProcessor<NodeContent, Geometry>
    {
        [DefaultValue(false)]
        public bool GenerateNormals { get; set; }

        [DefaultValue(false)]
        public bool GenerateTextureCoordinates { get; set; }


        public override Geometry Process(NodeContent input, ContentProcessorContext context)
        {
            List<Vector3> positions = new List<Vector3>();
            List<ushort> indices = new List<ushort>();

            List<Vector3> normals = new List<Vector3>();
            List<Vector2> textureCoordinates = new List<Vector2>();

            ProcessNode(input, positions, indices);

            return new Geometry(positions, indices);
        }

        private static void ProcessNode(NodeContent input, List<Vector3> positions, List<ushort> indices)
        {
            if (input != null)
            {
                // Find mesh content
                MeshContent mesh = input as MeshContent;

                if (mesh != null)
                {
                    foreach (GeometryContent geometry in mesh.Geometry)
                    {
                        int currentVertex = positions.Count;

                        foreach (Vector3 position in geometry.Vertices.Positions)
                            positions.Add(Vector3.Transform(position, mesh.AbsoluteTransform));

                        foreach (int index in geometry.Indices)
                            indices.Add((ushort)(currentVertex + index));
                    }
                }

                foreach (NodeContent child in input.Children)
                    ProcessNode(child, positions, indices);
            }
        }
    }
}