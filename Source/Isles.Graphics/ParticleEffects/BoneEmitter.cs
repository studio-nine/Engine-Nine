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
    public sealed class BoneEmitter : IParticleEmitter
    {
        static Random random = new Random();

        public Matrix[] Bones { get; set; }

        public ParticleVertex Emit(GameTime time, float lerpAmount)
        {
            ParticleVertex result;

            result.Time = 0;
            result.Random = Color.White;
            result.Velocity = Vector3.Zero;
            result.Position = Vector3.Zero;
            
            return result;
        }
    }
}
