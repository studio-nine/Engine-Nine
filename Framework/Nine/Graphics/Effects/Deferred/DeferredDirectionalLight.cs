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
    public partial class DeferredDirectionalLight : IDeferredLight, IEffectMatrices, IEffectTexture, IDirectionalLight
    {
        Quad primitive;

        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;
                viewProjectionInverse = Matrix.Invert(view * projection);
                eyePosition = Matrix.Invert(view).Translation;
            }
        }
        private Matrix view;

        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;
                viewProjectionInverse = Matrix.Invert(view * projection);
            }
        }
        private Matrix projection;

		private void OnCreated() 
        {
            primitive = GraphicsResources<Quad>.GetInstance(GraphicsDevice);
            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
        }

		private void OnClone(DeferredDirectionalLight cloneSource) 
        {
            view = cloneSource.view;
            projection = cloneSource.projection;
        }

		private void OnApplyChanges() 
        {
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
        
        Vector3 IDirectionalLight.SpecularColor
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
            return false;
        }
    }
}
