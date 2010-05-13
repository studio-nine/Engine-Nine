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

namespace Isles.Graphics.ParticleEffects
{
    internal partial class ParticleSystemEffect
    {
        public ParticleSystemEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();
        }
    }
}
