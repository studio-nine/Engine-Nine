#region File Description
//-----------------------------------------------------------------------------
// GeometricPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Primitives
{
    internal class Grid : Primitive
    {
        public float Width { get; private set; }
        public float Height { get; private set; }

        public int ResolutionU { get; private set; }
        public int ResolutionV { get; private set; }


        public Grid(GraphicsDevice graphicsDevice) : this(graphicsDevice, 1, 1, 8, 8) { }
        
        public Grid(GraphicsDevice graphicsDevice, float width, float height, int ru, int rv)                                
        {
            if (width <= 0 || height <= 0 || ru < 1 || rv < 1)
                throw new ArgumentException();

            
            Width = width;
            Height = height;
            ResolutionU = ru;
            ResolutionV = rv;

            float incU = width / ru;
            float incV = height / rv;

            for (int u = 0; u <= ru; u++)
            {
                AddVertex(new Vector3(-Height / 2, u * incU - width / 2, 0), Vector3.UnitZ);
                AddVertex(new Vector3(Height / 2, u * incU - width / 2, 0), Vector3.UnitZ);
            }

            for (int v = 0; v <= rv; v++)
            {
                AddVertex(new Vector3(v * incV - height / 2, -width / 2, 0), Vector3.UnitZ);
                AddVertex(new Vector3(v * incV - height / 2, height / 2, 0), Vector3.UnitZ);
            }

            InitializePrimitive(graphicsDevice);

            BasicEffect.LightingEnabled = false;
        }

        public override void Draw(Effect effect)
        {
            GraphicsDevice graphicsDevice = effect.GraphicsDevice;

            // Set our vertex declaration, vertex buffer, and index buffer.
            graphicsDevice.SetVertexBuffer(VertexBuffer);

            graphicsDevice.Indices = IndexBuffer;

            // Draw the model, using the specified effect.
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, Vertices.Count / 2);
            }
        }
    }
}
