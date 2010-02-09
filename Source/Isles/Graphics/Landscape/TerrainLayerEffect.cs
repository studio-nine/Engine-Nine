﻿#region Copyright 2009 (c) Nightin Games
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


namespace Isles.Graphics.Landscape
{
    public class TerrainLayerEffect : Isles.Graphics.Effects.BasicTextureEffect
    {
        public Texture2D AlphaTexture { get; set; }

        public Vector3 DiffuseColor { get; set; }
        public Vector3 EmissiveColor { get; set; }
        public Vector3 AmbientLightColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float SpecularPower { get; set; }
        public DirectionalLight LightSource { get; set; }


        public TerrainLayerEffect()
        {
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            AmbientLightColor = Vector3.One * 0.5f;
            SpecularColor = Vector3.Zero;
            SpecularPower = 32;

            LightSource = new DirectionalLight();
        }

        protected override void LoadContent()
        {
            Effect = InternalContents.TerrainLayerEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            Vector2 scale;

            scale.X = TessellationU;
            scale.Y = TessellationV;

            Effect.Parameters["Texture"].SetValue(Texture);
            Effect.Parameters["AlphaTexture"].SetValue(AlphaTexture);
            Effect.Parameters["TextureScale"].SetValue(scale);
            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            Effect.Parameters["Alpha"].SetValue(Alpha);
            Effect.Parameters["DiffuseColor"].SetValue(DiffuseColor);
            Effect.Parameters["SpecularColor"].SetValue(SpecularColor);
            Effect.Parameters["SpecularPower"].SetValue(SpecularPower);
            Effect.Parameters["EmissiveColor"].SetValue(EmissiveColor);
            Effect.Parameters["AmbientLightColor"].SetValue(AmbientLightColor);
            Effect.Parameters["LightDiffuseColor"].SetValue(LightSource.DiffuseColor);
            Effect.Parameters["LightDirection"].SetValue(Vector3.Normalize(LightSource.Direction));
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