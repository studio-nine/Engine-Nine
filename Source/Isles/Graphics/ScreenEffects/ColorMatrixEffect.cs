#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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
using Microsoft.Xna.Framework.Input;
#endregion


namespace Isles.Graphics.ScreenEffects
{
    /// <summary>
    /// A post processing screen effect that transforms the color of the whole screen.
    /// </summary>
    public partial class ColorMatrixEffect
    {
        /// <summary>
        /// Creates a new instance of color matrix post processing.
        /// </summary>
        public ColorMatrixEffect(GraphicsDevice graphicsDevice) : this(graphicsDevice, null) { }

        /// <summary>
        /// Creates a new instance of color matrix post processing.
        /// </summary>
        public ColorMatrixEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) :
            base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();
        }
    }
}
