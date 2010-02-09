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
    public class BasicSkinnedEffect : ModelEffect
    {
        public Effect Effect { get; private set; }


        protected override void LoadContent()
        {
            Effect = InternalContents.BasicSkinnedEffect(GraphicsDevice);
        }

        protected override bool Begin(GameTime time)
        {
            Effect.Parameters["Texture"].SetValue(Texture);
            Effect.Parameters["Bones"].SetValue(Bones);
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            
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
