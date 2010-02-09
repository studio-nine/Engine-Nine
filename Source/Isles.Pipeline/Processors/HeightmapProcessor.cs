#region Copyright 2009 (c) Nightin Games
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
using Isles.Graphics.Landscape;
#endregion


namespace Isles.Pipeline.Processors
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName="Heightmap Processor - Isles")]
    public class HeightmapProcessor : ContentProcessor<Texture2DContent, TerrainGeometry>
    {
        /// <summary>
        /// Controls the scale of the terrain. This will be the distance between
        /// vertices in the finished terrain mesh.
        /// </summary>
        [DefaultValue(1.0f)]
        [Description("The distance between vertices in the finished terrain mesh.")]
        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        private float scale = 1.0f;


        /// <summary>
        /// Controls the height of the terrain. The heights of the vertices in the
        /// finished mesh will vary between 0 and -Bumpiness.
        /// </summary>
        [DefaultValue(20.0f)]
        [Description("Controls the height of the terrain.")]
        public float Bumpiness
        {
            get { return bumpiness; }
            set { bumpiness = value; }
        }
        private float bumpiness = 20.0f;


        /// <summary>
        /// Generates a terrain mesh from an input heightfield texture.
        /// </summary>
        public override TerrainGeometry Process(Texture2DContent input, ContentProcessorContext context)
        {
            PixelBitmapContent<Alpha8> heightfield;

            MeshBuilder builder = MeshBuilder.StartMesh("Terrain");

            // Convert the input texture to float format, for ease of processing.
            GrayScaleTextureProcessor grayProcessor = new GrayScaleTextureProcessor();
            input = grayProcessor.Process(input, context) as Texture2DContent;
                                    
            heightfield = (PixelBitmapContent<Alpha8>)input.Mipmaps[0];

            if (heightfield.Width * heightfield.Height > ushort.MaxValue)
                throw new InvalidContentException("Input texture too large for a heightmap");

            // Create the terrain vertices.
            for (int y = 0; y < heightfield.Height; y++)
            {
                for (int x = 0; x < heightfield.Width; x++)
                {
                    Vector3 position;

                    position.X = scale * (x - ((heightfield.Width - 1) / 2.0f));
                    position.Y = scale * (y - ((heightfield.Height - 1) / 2.0f));
                    position.Z = heightfield.GetPixel(x, y).ToAlpha() * bumpiness;

                    builder.CreatePosition(position);
                }
            }
            

            // Create a vertex channel for holding texture coordinates.
            int texCoordId = builder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));

            // Create the individual triangles that make up our terrain.
            for (int y = 0; y < heightfield.Height - 1; y++)
            {
                for (int x = 0; x < heightfield.Width - 1; x++)
                {
                    AddVertex(builder, texCoordId, heightfield.Width, x, y);
                    AddVertex(builder, texCoordId, heightfield.Width, x + 1, y);
                    AddVertex(builder, texCoordId, heightfield.Width, x + 1, y + 1);

                    AddVertex(builder, texCoordId, heightfield.Width, x, y);
                    AddVertex(builder, texCoordId, heightfield.Width, x + 1, y + 1);
                    AddVertex(builder, texCoordId, heightfield.Width, x, y + 1);
                }
            }

            // Chain to the ModelProcessor so it can convert the mesh we just generated.
            MeshContent terrainMesh = builder.FinishMesh();


            // Compute tangents
            MeshHelper.CalculateTangentFrames(terrainMesh, VertexChannelNames.TextureCoordinate(0),
                                                           VertexChannelNames.Tangent(0), null);

            // Return the generated terrain
            return TerrainFromMesh(terrainMesh, heightfield.Width, heightfield.Height, scale);
        }


        /// <summary>
        /// Helper for adding a new triangle vertex to a MeshBuilder,
        /// along with an associated texture coordinate value.
        /// </summary>
        void AddVertex(MeshBuilder builder, int texCoordId, int w, int x, int y)
        {
            builder.SetVertexChannelData(texCoordId, new Vector2(x, y));

            builder.AddTriangleVertex(x + y * w);
        }


        /// <summary>
        /// Creates a terrain geometry from an input mesh.
        /// </summary>
        TerrainGeometry TerrainFromMesh(MeshContent terrainMesh, int terrainWidth, int terrainLength, float terrainScale)
        {
            // create new arrays of the requested size.
            Vector3[] normals = new Vector3[terrainWidth * terrainLength];
            Vector3[] tangents = new Vector3[terrainWidth * terrainLength];
            Vector3[] positions = new Vector3[terrainWidth * terrainLength];


            // to fill those arrays, we'll look at the position and normal data
            // contained in the terrainMesh.
            GeometryContent geometry = terrainMesh.Geometry[0];


            // we'll go through each vertex....
            for (int i = 0; i < geometry.Vertices.VertexCount; i++)
            {
                // ... and look up its position and normal.
                Vector3 position = geometry.Vertices.Positions[i];
                Vector3 normal = (Vector3)geometry.Vertices.Channels[VertexChannelNames.Normal(0)][i];
                Vector3 tangent = (Vector3)geometry.Vertices.Channels[VertexChannelNames.Tangent(0)][i];

                // from the position's X and Z value, we can tell what X and Y
                // coordinate of the arrays to put the height and normal into.
                int arrayX = (int)((position.X / terrainScale) + (terrainWidth - 1) / 2.0f);
                int arrayY = (int)((position.Y / terrainScale) + (terrainLength - 1) / 2.0f);

                positions[arrayX + arrayY * terrainWidth] = position;
                normals[arrayX + arrayY * terrainWidth] = normal;
                tangents[arrayX + arrayY * terrainWidth] = tangent;
            }


            Vector3 dimension;

            dimension.X = (terrainWidth - 1) * terrainScale;
            dimension.Y = (terrainLength - 1) * terrainScale;
            dimension.Z = bumpiness;

            return new TerrainGeometry(positions, normals, tangents, terrainWidth, terrainLength, dimension);
        }
    }
}
