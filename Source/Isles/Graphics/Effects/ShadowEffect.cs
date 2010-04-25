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
    public partial class ShadowEffect
    {
        public bool FogEnabled
        {
            get { return FogMask > 0.5f; }
            set { FogMask = (value ? 1.0f : 0.0f); }
        }
        
        public Matrix View
        {
            get { return ViewMatrix; }
            set { ViewMatrix = value; EyePosition = Matrix.Invert(value).Translation; }
        }

        public Matrix LightProjection
        {
            get { return LightProjectionMatrix; }
            set { LightProjectionMatrix = value; FarClip = Math.Abs(value.M43 / (Math.Abs(value.M33) - 1)); }
        }

        public ShadowEffect(GraphicsDevice graphicsDevice) : 
                this(graphicsDevice, null)
        {
        }
        
        public ShadowEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) : 
                base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();
        }
    }
}
