#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Isles.Graphics.Effects
{
    public partial class ShadowEffect
    {
        public bool FogEnabled
        {
            get { return fogMask > 0.5f; }
            set { fogMask = (value ? 1.0f : 0.0f); }
        }

        public ShadowEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();
        }

        protected override void OnApply()
        {
            farClip = Math.Abs(LightProjection.M43 / (Math.Abs(LightProjection.M33) - 1));
            eyePosition = Matrix.Invert(View).Translation;

            base.OnApply();
        }
    }
}
