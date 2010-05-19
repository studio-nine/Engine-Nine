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

namespace Nine.Graphics.Effects
{
    public partial class DecalEffect : IEffectMatrices
    {
        public Vector3 Position { get; set; }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public BoundingBox BoundingBox
        {
            get
            {
                float size = (float)(Math.Max(Scale.X, Scale.Y) * Math.Sqrt(2) * 0.5f);

                return new BoundingBox(
                    Position - new Vector3(size, size, float.MaxValue),
                    Position + new Vector3(size, size, float.MaxValue));
            }
        }

        public DecalEffect(GraphicsDevice graphics) : base(GetSharedEffect(graphics))
        {
            InitializeComponent();
        }

        protected override void OnApply()
        {
            Matrix matrix = Matrix.CreateTranslation(-Position + new Vector3(Scale.X / 2, Scale.Y / 2, 0));
            matrix *= Matrix.CreateRotationZ(-Rotation);
            matrix *= Matrix.CreateScale(1.0f / Scale.X, 1.0f / Scale.Y, 1);

            textureTransform = matrix;

            base.OnApply();
        }
    }
}
