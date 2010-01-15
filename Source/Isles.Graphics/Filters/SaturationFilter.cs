#region File Description
//-----------------------------------------------------------------------------
// BloomComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics.Filters
{
    public sealed class SaturationFilter : Filter
    {
        private Effect effect;

        public float Saturation { get; set; }


        public SaturationFilter()
        {
            Saturation = 0.5f;
        }

        protected override void LoadContent()
        {
            effect = InternalContents.SaturationEffect(GraphicsDevice);
        }

        protected override void Begin(Texture2D input)
        {
            effect.Parameters["Saturation"].SetValue(Saturation);

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
        }

        protected override void End()
        {
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}

