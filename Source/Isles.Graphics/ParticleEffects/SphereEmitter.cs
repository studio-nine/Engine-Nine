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
    public sealed class SphereEmitter : IParticleEmitter
    {
        static Random random = new Random();

        public BoundingSphere Sphere { get; set; }


        public SphereEmitter()
        {
            Sphere = new BoundingSphere(Vector3.Zero, 1.0f);
        }


        public ParticleVertex Emit(GameTime time, float lerpAmount)
        {
            ParticleVertex result;

            result.Time = 0;
            result.Random = Color.White;
            result.Velocity = Vector3.Zero;

            double r = random.NextDouble() * Sphere.Radius;            
            double a = random.NextDouble() * Math.PI * 2;
            double b = random.NextDouble() * Math.PI - MathHelper.PiOver2;
            double rr = Math.Cos(b) * r;

            result.Position.X = (float)(rr * Math.Cos(a));
            result.Position.Y = (float)(rr * Math.Sin(a));
            result.Position.Z = (float)(r * Math.Sin(b));
            
            return result;
        }
    }
}
