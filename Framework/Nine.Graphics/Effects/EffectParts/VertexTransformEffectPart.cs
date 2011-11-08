#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    internal class VertexTransformEffectPart : LinkedEffectPart, IEffectMatrices
    {
        private uint DirtyMask = 0;
        
        private Matrix world;
        private EffectParameter worldParameter;
        private const uint worldDirtyMask = 1 << 0;

        private Matrix view;
        private Matrix projection;
        private EffectParameter worldViewProjectionParameter;
        private const uint worldViewProjectionDirtyMask = 1 << 1;

        [ContentSerializerIgnore]
        public Matrix World
        {
            get { return world; }
            set { world = value; DirtyMask |= worldDirtyMask; DirtyMask |= worldViewProjectionDirtyMask; }
        }

        [ContentSerializerIgnore]
        public Matrix View
        {
            get { return view; }
            set { view = value; DirtyMask |= worldViewProjectionDirtyMask; }
        }

        [ContentSerializerIgnore]
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; DirtyMask |= worldViewProjectionDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & worldDirtyMask) != 0)
            {
                if (worldParameter == null)
                    worldParameter = GetParameter("World");
                worldParameter.SetValue(world);
                DirtyMask &= ~worldDirtyMask;
            }

            if ((DirtyMask & worldViewProjectionDirtyMask) != 0)
            {
                if (worldViewProjectionParameter == null)
                    worldViewProjectionParameter = GetParameter("WorldViewProjection");

                Matrix wvp;
                Matrix.Multiply(ref world, ref view, out wvp);
                Matrix.Multiply(ref wvp, ref projection, out wvp);

                worldViewProjectionParameter.SetValue(wvp);
                DirtyMask &= ~worldViewProjectionDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new VertexTransformEffectPart();
        }
    }

#endif
}
