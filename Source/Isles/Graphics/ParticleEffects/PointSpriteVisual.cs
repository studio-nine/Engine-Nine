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
    public abstract class PointSpriteVisual : IParticleVisual
    {
        #region IParticleVisual Members

        public void Draw(GraphicsDevice graphics, ParticleVertex[] particles, Matrix view, Matrix projection)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    public sealed class BasicPointSpriteVisual : PointSpriteVisual
    {
    }

    public sealed class ExtendedPointSpriteVisual : PointSpriteVisual
    {
    }


    /*
    public sealed class BasicParticleEffect : ParticleEffect
    {
        // How long these particles will last.
        public TimeSpan Duration { get; set; }


        // If greater than zero, some particles will last a shorter time than others.
        public float DurationRandomness { get; set; }


        // Range of values controlling how much X and Z axis velocity to give each
        // particle. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        public float MinHorizontalVelocity { get; set; }
        public float MaxHorizontalVelocity { get; set; }


        // Range of values controlling how much Y axis velocity to give each particle.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        public float MinVerticalVelocity { get; set; }
        public float MaxVerticalVelocity { get; set; }


        // Direction and strength of the gravity effect. Note that this can point in any
        // direction, not just down! The fire effect points it upward to make the flames
        // rise, and the smoke plume points it sideways to simulate wind.
        public Vector3 Gravity { get; set; }


        // Controls how the particle velocity will change over their lifetime. If set
        // to 1, particles will keep going at the same speed as when they were created.
        // If set to 0, particles will come to a complete stop right before they die.
        // Values greater than 1 make the particles speed up over time.
        public float EndVelocity { get; set; }


        // Range of values controlling the particle color and alpha. Values for
        // individual particles are randomly chosen from somewhere between these limits.
        public Color MinColor { get; set; }
        public Color MaxColor { get; set; }


        // Range of values controlling how fast the particles rotate. Values for
        // individual particles are randomly chosen from somewhere between these
        // limits. If both these values are set to 0, the particle system will
        // automatically switch to an alternative shader technique that does not
        // support rotation, and thus requires significantly less GPU power. This
        // means if you don't need the rotation effect, you may get a performance
        // boost from leaving these values at 0.
        public float MinRotateSpeed { get; set; }
        public float MaxRotateSpeed { get; set; }


        // Range of values controlling how big the particles are when first created.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        public float MinStartSize { get; set; }
        public float MaxStartSize { get; set; }


        // Range of values controlling how big particles become at the end of their
        // life. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        public float MinEndSize { get; set; }
        public float MaxEndSize { get; set; }


        public Effect Effect { get; private set; }


        public BasicParticleEffect()
        {
            Duration = TimeSpan.FromSeconds(2);

            DurationRandomness = 1;

            MinHorizontalVelocity = 0;
            MaxHorizontalVelocity = 15;

            MinVerticalVelocity = -10;
            MaxVerticalVelocity = 10;

            // Set gravity upside down, so the flames will 'fall' upward.
            Gravity = new Vector3(0, 0, -10);

            MinColor = new Color(255, 255, 255, 10);
            MaxColor = new Color(255, 255, 255, 40);

            MinStartSize = 5;
            MaxStartSize = 10;

            MinEndSize = 10;
            MaxEndSize = 40;
        }

        protected override void LoadContent()
        {
            Effect = InternalContents.BasicParticleEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            EffectParameterCollection parameters = Effect.Parameters;

            // Look up shortcuts for parameters that change every frame.
            parameters["View"].SetValue(View);
            parameters["Projection"].SetValue(Projection);
            parameters["ViewportHeight"].SetValue(GraphicsDevice.Viewport.Height);
            parameters["CurrentTime"].SetValue((float)time.TotalGameTime.TotalSeconds);

            // Set the values of parameters that do not change.
            parameters["Duration"].SetValue((float)Duration.TotalSeconds);
            parameters["DurationRandomness"].SetValue(DurationRandomness);
            parameters["Gravity"].SetValue(Gravity);
            parameters["EndVelocity"].SetValue(EndVelocity);
            parameters["MinColor"].SetValue(MinColor.ToVector4());
            parameters["MaxColor"].SetValue(MaxColor.ToVector4());
            parameters["RotateSpeed"].SetValue(new Vector2(MinRotateSpeed, MaxRotateSpeed));
            parameters["StartSize"].SetValue(new Vector2(MinStartSize, MaxStartSize));
            parameters["EndSize"].SetValue(new Vector2(MinEndSize, MaxEndSize));
            parameters["Texture"].SetValue(Texture);

            // Choose the appropriate effect technique. If these particles will never
            // rotate, we can use a simpler pixel shader that requires less GPU power.
            string techniqueName;

            if ((MinRotateSpeed == 0) && (MaxRotateSpeed == 0))
                techniqueName = "NonRotatingParticles";
            else
                techniqueName = "RotatingParticles";

            Effect.CurrentTechnique = Effect.Techniques[techniqueName];

            Effect.Begin();
            Effect.CurrentTechnique.Passes[0].Begin();

            return true;
        }

        public override void End()
        {
            Effect.CurrentTechnique.Passes[0].End();
            Effect.End();
        }
    }
     */
}
