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
    public sealed class PointEmitter : IParticleEmitter
    {
        private Vector3 previousPosition;

        public Vector3 Position { get; set; }

        /// <summary>
        /// When position change has exceeded this value, PointEmitter will
        /// not interpolate between the old and new positions.
        /// </summary>
        public float MaxLerpDistance { get; set; }


        public PointEmitter()
        {
            MaxLerpDistance = 5;
            previousPosition = Vector3.UnitZ * float.MaxValue;
        }


        public ParticleVertex Emit(GameTime time, float lerpAmount)
        {
            ParticleVertex result;

            result.Random = Color.White;
            result.Time = 0;

            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;

            Vector3 increment = Position - previousPosition;

            if (increment.LengthSquared() < MaxLerpDistance * MaxLerpDistance)
            {
                result.Velocity = (Position - previousPosition) / elapsedTime;
                result.Position = Vector3.Lerp(previousPosition, Position, lerpAmount);
            }
            else
            {
                result.Velocity = Vector3.Zero;
                result.Position = Position;
            }

            previousPosition = Position;

            return result;
        }
    }
}
