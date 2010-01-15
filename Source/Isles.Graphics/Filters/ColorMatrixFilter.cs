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
    public sealed class ColorMatrixFilter : Filter
    {
        private Effect effect;

        public Matrix Matrix { get; set; }


        public ColorMatrixFilter()
        {
            Matrix = Matrix.Identity;
        }

        protected override void LoadContent()
        {
            effect = InternalContents.ColorMatrixEffect(GraphicsDevice);
        }

        protected override void Begin(Texture2D input)
        {
            effect.Parameters["Matrix"].SetValue(Matrix);

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

