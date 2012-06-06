#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Nine.Studio;
using Nine.Studio.Controls;
using Nine.Studio.Visualizers;
using Nine.Studio.Extensibility;
using Nine.Content.Pipeline.Graphics.ParticleEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ParticleEffects.Design
{
    public class ParticleEffectVisualizer : GraphicsDocumentVisualizer<ParticleEffect>
    {
        BasicEffect effect;
        Stopwatch timer;


        // Vertex positions and colors used to display a spinning triangle.
        public readonly VertexPositionColor[] Vertices =
        {
            new VertexPositionColor(new Vector3(-1, -1, 0), Color.Black),
            new VertexPositionColor(new Vector3( 1, -1, 0), Color.Black),
            new VertexPositionColor(new Vector3( 0,  1, 0), Color.Black),
        };

        static int i = 0;
        
        protected override void Draw(TimeSpan elapsedTime)
        {
            // Create our effect.
            if (effect == null)
            {
                effect = new BasicEffect(GraphicsDevice);

                effect.VertexColorEnabled = true;

                timer = Stopwatch.StartNew();

                Vertices[0].Color = i++ == 0 ? Color.Yellow : Color.Red;
            }
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Spin the triangle according to how much time has passed.
            float time = (float)timer.Elapsed.TotalSeconds;

            float yaw = time * 0.7f;
            float pitch = time * 0.8f;
            float roll = time * 0.9f;

            // Set transform matrices.
            float aspect = GraphicsDevice.Viewport.AspectRatio;

            effect.World = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

            effect.View = Matrix.CreateLookAt(new Vector3(0, 0, -5),
                                              Vector3.Zero, Vector3.Up);

            effect.Projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 10);

            // Set renderstates.
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Draw the triangle.
            effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
                                              Vertices, 0, 1);

            Surface.Invalidate();
        }
    }
}
