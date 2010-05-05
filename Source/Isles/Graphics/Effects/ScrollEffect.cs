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
#endregion

namespace Isles.Graphics.Effects
{
    public partial class ScrollEffect : IEffectMatrices
    {
        public float Speed { get; set; }
        public float Direction { get; set; }

        private TimeSpan startTime = TimeSpan.Zero;
                        
        public ScrollEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();

            Speed = 1.0f;
            TextureScale = Vector2.One;
        }

        public void Update(GameTime time)
        {
            if (startTime == TimeSpan.Zero)
                startTime = time.TotalGameTime;

            TimeSpan duration = time.TotalGameTime - startTime;

            float dx = (float)(duration.TotalSeconds * Speed * Math.Cos(Direction));
            float dy = (float)(duration.TotalSeconds * Speed * Math.Sin(Direction));


            textureOffset = new Vector2(dx, dy);            
        }
    }
}
