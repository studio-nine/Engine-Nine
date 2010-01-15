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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Graphics.Landscape
{
    public class WaterSurfaceEffect : GraphicsEffect
    {
        [ContentSerializerIgnore]
        public Matrix ReflectionViewProjection { get; set; }
        [ContentSerializer(Optional=true)]
        public float Direction { get; set; }
        [ContentSerializer(Optional=true)]
        public float Speed { get; set; }
        public float WaveTessellationU { get; set; }
        public float WaveTessellationV { get; set; }

        [ContentSerializerIgnore]
        public Texture2D WaveTexture { get; set; }
        [ContentSerializerIgnore]
        public Texture2D ReflectionTexture { get; set; }
        [ContentSerializerIgnore]
        public Texture2D RefractionTexture { get; set; }

        public Vector3 DiffuseColor { get; set; }
        public Vector3 EmissiveColor { get; set; }
        public Vector3 AmbientLightColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float SpecularPower { get; set; }
        public float Scatter { get; set; }

        [ContentSerializer(Optional=true)]
        public PointLight LightSource { get; set; }

        public Effect Effect { get; private set; }

        public WaterSurfaceEffect()
        {
            Scatter = 50.0f;
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            AmbientLightColor = Vector3.Zero;
            SpecularColor = Vector3.Zero;
            SpecularPower = 32;
            Speed = 1.0f;

            WaveTessellationU = WaveTessellationV = 8;

            LightSource = new PointLight();
        }

        protected override void LoadContent()
        {
            Effect = InternalContents.DebugShader(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            Vector2 waveScale, distortionScale;

            waveScale.X = WaveTessellationU;
            waveScale.Y = WaveTessellationV;

            distortionScale.X = Scatter / ReflectionTexture.Width;
            distortionScale.Y = Scatter / ReflectionTexture.Height;

            Vector2 offset;

            offset.X = (float)(time.TotalGameTime.TotalSeconds * Speed * Math.Cos(Direction));
            offset.Y = (float)(time.TotalGameTime.TotalSeconds * Speed * Math.Sin(Direction));


            Effect.Parameters["DistortionScale"].SetValue(distortionScale);
            Effect.Parameters["WaveTextureScale"].SetValue(waveScale);
            Effect.Parameters["WaveTextureOffset"].SetValue(offset);
            Effect.Parameters["WaveTexture"].SetValue(WaveTexture);
            Effect.Parameters["ReflectionTexture"].SetValue(ReflectionTexture);
            Effect.Parameters["RefractionTexture"].SetValue(RefractionTexture);
            Effect.Parameters["ReflectionViewProjection"].SetValue(ReflectionViewProjection);
            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            Effect.Parameters["DiffuseColor"].SetValue(DiffuseColor);
            Effect.Parameters["SpecularColor"].SetValue(SpecularColor);
            Effect.Parameters["SpecularPower"].SetValue(SpecularPower);
            Effect.Parameters["EmissiveColor"].SetValue(EmissiveColor);
            Effect.Parameters["AmbientLightColor"].SetValue(AmbientLightColor);
            Effect.Parameters["LightDiffuseColor"].SetValue(LightSource.DiffuseColor);
            Effect.Parameters["LightPosition"].SetValue(LightSource.Position);
            Effect.Parameters["LightSpecularColor"].SetValue(LightSource.SpecularColor);
            Effect.Parameters["EyePosition"].SetValue(Matrix.Invert(View).Translation);


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
}
