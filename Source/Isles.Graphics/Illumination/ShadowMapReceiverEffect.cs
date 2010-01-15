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


namespace Isles.Graphics.Illumination
{
    public class ShadowMapReceiverEffect : GraphicsEffect
    {
        public float Bias { get; set; }
        public float Alpha { get; set; }
        public Effect Effect { get; private set; }
        public Matrix LightViewProjection { get; set; }


        public ShadowMapReceiverEffect()
        {
            Alpha = 0.6f;
            Bias = 0.03f;
            LightViewProjection = Matrix.Identity;
        }

        protected override void LoadContent()
        {
            CompiledEffect compiled = Effect.CompileEffectFromFile("ShadowReceiver.fx", null, null, CompilerOptions.None, TargetPlatform.Windows);

            Effect = new Effect(GraphicsDevice, compiled.GetEffectCode(), CompilerOptions.None, null);

            //Effect = InternalContents.ShadowReceiverEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            GraphicsDevice.SamplerStates[0].BorderColor = Color.White;
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Border;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Border;
            

            if (VertexSkinningEnabled)
                Effect.Parameters["Bones"].SetValue(Bones);
            else
                Effect.Parameters["World"].SetValue(World);

            Effect.Parameters["Alpha"].SetValue(Alpha);
            Effect.Parameters["Fudge"].SetValue(Bias);
            Effect.Parameters["ShadowMap"].SetValue(Texture);
            Effect.Parameters["ViewProjection"].SetValue(Matrix.Multiply(View, Projection));
            Effect.Parameters["LightViewProjection"].SetValue(LightViewProjection);

            
            int pass = VertexSkinningEnabled ? 1 : 0;
            
            Effect.Begin();
            Effect.CurrentTechnique.Passes[pass].Begin();

            return true;
        }

        public override void End()
        {
            int pass = VertexSkinningEnabled ? 1 : 0;

            Effect.CurrentTechnique.Passes[pass].End();
            Effect.End();

            
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }
    }
}
