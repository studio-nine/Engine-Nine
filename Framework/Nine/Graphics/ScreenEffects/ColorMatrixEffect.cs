#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    /// <summary>
    /// A post processing screen effect that transforms the color of the whole screen.
    /// </summary>
    public partial class ColorMatrixEffect
    {
        /// <summary>
        /// Creates a new instance of color matrix post processing.
        /// </summary>
        public ColorMatrixEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics)) 
        {
            InitializeComponent();
        }
    }

#endif
}
