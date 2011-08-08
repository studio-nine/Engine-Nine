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
    public partial class DeferredSpotLight : IDeferredLight, IEffectMatrices, IEffectTexture, ISpotLight
    {
        Centrum primitive;

        Matrix worldInverse;
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
                        
        public float InnerAngle
        {
            get { return mInnerAngle; }
            set
            {
                mInnerAngle = value;
                innerAngle = (float)Math.Cos(mInnerAngle * 0.5);
            }
        }
        private float mInnerAngle;

        public float OuterAngle
        {
            get { return mOuterAngle; }
            set
            {
                mOuterAngle = value;
                outerAngle = (float)Math.Cos(mOuterAngle * 0.5);
            }
        }
        private float mOuterAngle;

        private void OnCreated()
        {
            primitive = GraphicsResources<Centrum>.GetInstance(GraphicsDevice);

            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
            Range = 10;
            Attenuation = MathHelper.E;
            InnerAngle = MathHelper.PiOver4;
            OuterAngle = MathHelper.PiOver2;
            Falloff = 1;
        }

        private void OnClone(DeferredSpotLight cloneSource)
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

            // Update world only when position/range/outer angle changed.
            if ((this.dirtyFlag & PositionDirtyFlag) != 0 ||
                (this.dirtyFlag & RangeDirtyFlag) != 0 ||
                (this.dirtyFlag & outerAngleDirtyFlag) != 0)
            {
                float radius = (float)Math.Tan(OuterAngle * 0.5) * Range;
                world = Matrix.CreateScale(radius, radius, Range) *
                        Matrix.CreateTranslation(0, 0, -Range) *
                        MatrixHelper.CreateRotation(Vector3.UnitZ, -Direction) *
                        Matrix.CreateTranslation(Position);
                
                // Compute invert for bounding test
                worldInverse = Matrix.Invert(world);
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
            Vector3 local = Vector3.Transform(point, worldInverse);
            if (local.Z < 0 || local.Z > 1)
                return false;

            return local.X * local.X + local.Y * local.Y <= (1 - local.Z) * (1 - local.Z);
        }
    }
}
