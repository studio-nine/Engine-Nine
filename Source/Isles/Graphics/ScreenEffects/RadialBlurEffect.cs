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


namespace Isles.Graphics.ScreenEffects
{
    /// <summary>
    /// A post processing screen effect that blurs the whole screen radially.
    /// </summary>
    public partial class RadialBlurEffect
    {
        /// <summary>
        /// Creates a new instance of radial blur post processing.
        /// </summary>
        public RadialBlurEffect(GraphicsDevice graphicsDevice) :
            this(graphicsDevice, null)
        {
        }

        /// <summary>
        /// Creates a new instance of radial blur post processing.
        /// </summary>
        public RadialBlurEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) :
            base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();
        }
    }
}
