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


namespace Isles.Graphics.Landscape
{
    public class SkyBoxEffect : GraphicsEffect
    {
        public float FarClip { get; set; }
        public Vector3 AmbientLightColor { get; set; }
        

        public Effect Effect { get; private set; }

        public SkyBoxEffect()
        {
            FarClip = 100;
            AmbientLightColor = Vector3.One;
        }

        public SkyBoxEffect(TextureCube cube)
        {
            FarClip = 100;
            Texture = cube;
            AmbientLightColor = Vector3.One;
        }        

        protected override void LoadContent()
        {
            Effect = InternalContents.SkyBoxEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["FarClip"].SetValue(FarClip);
            Effect.Parameters["CubeTexture"].SetValue(Texture);
            Effect.Parameters["Projection"].SetValue(Projection);
            Effect.Parameters["AmbientColor"].SetValue(AmbientLightColor);


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
