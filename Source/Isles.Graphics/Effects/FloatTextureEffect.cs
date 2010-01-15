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
    public class FloatTextureEffect : BasicTextureEffect
    {
        public float Speed { get; set; }
        public float Direction { get; set; }
        public Texture FloatTexture { get; set; }

        private TimeSpan startTime = TimeSpan.Zero;


        public FloatTextureEffect()
        {
            Speed = 1.0f;
        }

        public FloatTextureEffect(Texture2D texture) : base(texture)
        {
            Speed = 1.0f;
        }


        protected override void LoadContent()
        {
            Effect = InternalContents.FloatTextureEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            if (startTime == TimeSpan.Zero)
                startTime = time.TotalGameTime;

            TimeSpan duration = time.TotalGameTime - startTime;

            Vector2 offset, scale;

            offset.X = (float)(duration.TotalMilliseconds * Speed * Math.Cos(Direction) * 0.0001f);
            offset.Y = (float)(duration.TotalMilliseconds * Speed * Math.Sin(Direction) * 0.0001f);

            scale.X = TessellationU;
            scale.Y = TessellationV;


            Effect.Parameters["TextureOffset"].SetValue(offset);
            Effect.Parameters["TextureScale"].SetValue(scale);
            Effect.Parameters["Texture"].SetValue(Texture != null ? Texture : FloatTexture);
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
