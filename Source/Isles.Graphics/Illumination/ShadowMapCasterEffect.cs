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
    public class ShadowMapCasterEffect : ModelEffect
    {
        public Effect Effect { get; private set; }
        public Matrix LightViewProjection { get; set; }


        protected override void LoadContent()
        {
            Effect = InternalContents.ShadowCasterEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            if (VertexSkinningEnabled)
                Effect.Parameters["Bones"].SetValue(Bones);
            else
                Effect.Parameters["World"].SetValue(World);

            Effect.Parameters["ViewProjection"].SetValue(LightViewProjection);

            
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
        }
    }
}
