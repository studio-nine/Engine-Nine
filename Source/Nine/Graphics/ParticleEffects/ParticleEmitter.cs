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


namespace Nine.Graphics.ParticleEffects
{
    public sealed class ParticleEmitter
    {
        float timeLeftOver = 0;

        public float Emission { get; set; }
        public ISpacialEmitter SpacialEmitter { get; set; }


        public ParticleEmitter()
        {
            Emission = 100;
            SpacialEmitter = new PointEmitter();
        }

        public ParticleEmitter(float emission, ISpacialEmitter emitter)
        {
            Emission = emission;
            SpacialEmitter = emitter;
        }

        public IEnumerable<Vector3> Update(GameTime time)
        {
            if (SpacialEmitter != null)
            {
                // Work out how much time has passed since the previous update.
                float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;
                float timeBetweenParticles = 1.0f / Emission;

                if (elapsedTime > 0)
                {
                    // If we had any time left over that we didn't use during the
                    // previous update, add that to the current elapsed time.
                    float timeToSpend = timeLeftOver + elapsedTime;

                    // Counter for looping over the time interval.
                    float currentTime = -timeLeftOver;

                    // Create particles as long as we have a big enough time interval.
                    while (timeToSpend > timeBetweenParticles)
                    {
                        currentTime += timeBetweenParticles;
                        timeToSpend -= timeBetweenParticles;

                        // Work out the optimal position for this particle. This will produce
                        // evenly spaced particles regardless of the object speed, particle
                        // creation frequency, or game update rate.
                        float mu = currentTime / elapsedTime;

                        yield return SpacialEmitter.Emit(time, mu);
                    }

                    // Store any time we didn't use, so it can be part of the next update.
                    timeLeftOver = timeToSpend;
                }
            }
        }
    }
}
