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


namespace Isles.Graphics.Effects
{
    public class BasicTextureEffect : GraphicsEffect
    {
        public float TessellationU { get; set; }
        public float TessellationV { get; set; }
        public float Alpha { get; set; }
        public Effect Effect { get; protected set; }

        public BasicTextureEffect()
        {
            Alpha = 1.0f;
            TessellationU = TessellationV = 1.0f;
        }

        public BasicTextureEffect(Texture2D texture)
        {
            Texture = texture;
            Alpha = 1.0f;
            TessellationU = TessellationV = 1.0f;
        }


        protected override void LoadContent()
        {
            Effect = InternalContents.BasicTextureEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            Vector2 scale;

            scale.X = TessellationU;
            scale.Y = TessellationV;

            Effect.Parameters["TextureScale"].SetValue(scale);
            Effect.Parameters["Texture"].SetValue(Texture);
            Effect.Parameters["World"].SetValue(World);
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            Effect.Parameters["Alpha"].SetValue(Alpha);


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
