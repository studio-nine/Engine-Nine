#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Primitives;
#endregion


namespace Nine.Graphics.Effects.Deferred
{
    public partial class DeferredPointLight : IDeferredLight, IEffectMatrices, IEffectTexture, IPointLight
    {
        Sphere primitive;

        bool viewProjectionChanged;

        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                viewProjectionChanged = true;
            }
        }
        private Matrix view;

        public Matrix Projection
        {
            get { return projection; }
            set
            {
                viewProjectionChanged = true;
                projection = value;
            }
        }
        private Matrix projection;

        private void OnCreated()
        {
            primitive = GraphicsResources<Sphere>.GetInstance(GraphicsDevice);

            Range = 10;
            Attenuation = 1.0f / MathHelper.E;
            DiffuseColor = Vector3.One;
        }

        private void OnClone(DeferredPointLight cloneSource)
        {
            view = cloneSource.view;
            projection = cloneSource.projection;
        }

        private void OnApplyChanges()
        {
            if (viewProjectionChanged)
            {
                viewProjection = view * Projection;
                viewProjectionInverse = Matrix.Invert(viewProjection);
                eyePosition = Matrix.Invert(view).Translation;
                viewProjectionChanged = false;
            }
            halfPixel = new Vector2(0.5f / GraphicsDevice.Viewport.Width, 0.5f / GraphicsDevice.Viewport.Height);
        }

        VertexBuffer IDeferredLight.VertexBuffer
        {
            get { return primitive.VertexBuffer; }
        }

        IndexBuffer IDeferredLight.IndexBuffer
        {
            get { return primitive.IndexBuffer; }
        }

        Effect IDeferredLight.Effect
        {
            get { return this; }
        }

        Matrix IEffectMatrices.World
        {
            get { return Matrix.Identity; }
            set { }
        }

        Vector3 IPointLight.SpecularColor
        {
            get { return Vector3.Zero; }
            set { }
        }

        Texture2D IEffectTexture.Texture
        {
            get { return null; }
            set { }
        }

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.NormalMap)
                NormalBuffer = texture as Texture2D;
            else if (name == TextureNames.DepthMap)
                DepthBuffer = texture as Texture2D;
        }

        bool IDeferredLight.Contains(Vector3 point)
        {
            return Vector3.Subtract(point, Position).LengthSquared() < Range * Range;
        }
    }
}
