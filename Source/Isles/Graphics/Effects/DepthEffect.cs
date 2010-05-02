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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Isles.Graphics.Vertices;
#endregion

namespace Isles.Graphics.Effects
{
    public partial class DepthEffect : IEffectMatrices
    {
        public bool SkinningEnabled
        {
            get { return Parameters["ShaderIndex"].GetValueInt32() == 0; }
            set { Parameters["ShaderIndex"].SetValue(value ? 1 : 0); }
        }

        public DepthEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();
        }

        protected override void OnApply()
        {
            farClip = Math.Abs(Projection.M43 / (Math.Abs(Projection.M33) - 1));

            base.OnApply();
        }
    }
}
