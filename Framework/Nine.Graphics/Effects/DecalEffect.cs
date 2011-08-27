#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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
#if !WINDOWS_PHONE

    /// <summary>
    /// Represents an effect for drawing decals.
    /// </summary>
    public partial class DecalEffect : IEffectMatrices, IEffectTexture, IBoundable
    {
        private Matrix view;
        private Matrix projection;
        private Vector3 position;

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public float Rotation { get; set; }

        public Vector2 Scale { get; set; }

        public Matrix View
        {
            get { return view; }
            set { view = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

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

        private void OnCreated() 
        {

        }

        private void OnClone(DecalEffect cloneSource) 
        {
            Position = cloneSource.Position;
            Rotation = cloneSource.Rotation;
            Scale = cloneSource.Scale;
        }

        private void OnApplyChanges()
        {
            if ((dirtyFlag & worldViewProjectionDirtyFlag) != 0 ||
                (dirtyFlag & WorldDirtyFlag) != 0)
            {
                Matrix wvp;
                Matrix.Multiply(ref _World, ref view, out wvp);
                Matrix.Multiply(ref wvp, ref projection, out wvp);
                worldViewProjection = wvp;
            }

            Matrix matrix;
            Matrix rotation;
            Vector3 position = new Vector3(Scale.X / 2, Scale.Y / 2, 0);
            Vector3.Subtract(ref position, ref this.position, out position);
            Matrix.CreateTranslation(ref position, out matrix);
            Matrix.CreateRotationZ(-Rotation, out rotation);
            Matrix.Multiply(ref matrix, ref rotation, out matrix);
            Matrix.CreateScale(1.0f / Scale.X, 1.0f / Scale.Y, 1, out rotation);
            Matrix.Multiply(ref matrix, ref rotation, out matrix);

            textureTransform = matrix;
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Decal)
                Texture = texture as Texture2D;
        }
    }

#endif
}
