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
    public partial class DecalEffect
    {
        Vector2 scale = Vector2.One;
        Vector3 position;
        float rotation;

        public Vector3 Position
        {
            get { return position; }
            set { position = value; Update(); }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; Update(); }
        }

        public Vector2 Scale
        {
            get { return scale; }
            set { scale = value; Update(); }
        }

        public BoundingBox BoundingBox { get; private set; }

        public DecalEffect(GraphicsDevice graphicsDevice) : this(graphicsDevice, null) { }
        
        public DecalEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) : 
                base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();

            Update();
        }

        private void Update()
        {
            Matrix matrix = Matrix.CreateTranslation(-position + new Vector3(scale.X / 2, scale.Y / 2, 0));
            matrix *= Matrix.CreateRotationZ(-rotation);
            matrix *= Matrix.CreateScale(1.0f / scale.X, 1.0f / scale.Y, 1);

            TextureTransform = matrix;

            float size = (float)(Math.Max(scale.X, scale.Y) * Math.Sqrt(2) * 0.5f);

            BoundingBox = new BoundingBox(
                position - new Vector3(size, size, float.MaxValue),
                position + new Vector3(size, size, float.MaxValue));
        }
    }
}
