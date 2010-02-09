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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Graphics.ParticleEffects
{
    public sealed class BoxEmitter : IParticleEmitter
    {
        static Random random = new Random();

        public BoundingBox Box { get; set; }


        public BoxEmitter()
        {
            Box = new BoundingBox(Vector3.One * -1.0f, Vector3.One * 1.0f);
        }


        public ParticleVertex Emit(GameTime time, float lerpAmount)
        {
            ParticleVertex result;

            result.Time = 0;
            result.Random = Color.White;
            result.Velocity = Vector3.Zero;

            result.Position.X = Box.Min.X + (float)(random.NextDouble() * (Box.Max.X - Box.Min.X));
            result.Position.Y = Box.Min.Y + (float)(random.NextDouble() * (Box.Max.Y - Box.Min.Y));
            result.Position.Z = Box.Min.Z + (float)(random.NextDouble() * (Box.Max.Z - Box.Min.Z));
            
            return result;
        }
    }
}
