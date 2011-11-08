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

    internal class FogEffectPart : LinkedEffectPart, IEffectMatrices, IEffectFog
    {
        private uint DirtyMask = 0;
        
        private Vector3 fogColor;
        private EffectParameter fogColorParameter;
        private const uint fogColorDirtyMask = 1 << 0;

        private float fogStart;
        private float fogEnd;
        private Matrix world;
        private Matrix view;
        private EffectParameter fogVectorParameter;
        private const uint fogVectorDirtyMask = 1 << 1;

        [ContentSerializer(Optional = true)]
        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; DirtyMask |= fogColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float FogStart
        {
            get { return fogStart; }
            set { if (fogStart != value) { fogStart = value; DirtyMask |= fogVectorDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public float FogEnd
        {
            get { return fogEnd; }
            set { if (fogEnd != value) { fogEnd = value; DirtyMask |= fogVectorDirtyMask; } }
        }

        public FogEffectPart()
        {
            FogColor = Vector3.One;
            FogStart = 1;
            FogEnd = 100;
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & fogColorDirtyMask) != 0)
            {
                if (fogColorParameter == null)
                    fogColorParameter = GetParameter("FogColor");
                fogColorParameter.SetValue(fogColor);
                DirtyMask &= ~fogColorDirtyMask;
            }

            if ((DirtyMask & fogVectorDirtyMask) != 0)
            {
                if (fogVectorParameter == null)
                    fogVectorParameter = GetParameter("FogVector");

                Matrix worldView;
                Matrix.Multiply(ref world, ref view, out worldView);
                SetFogVector(ref worldView, fogStart, fogEnd, fogVectorParameter);
                DirtyMask &= ~fogVectorDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new FogEffectPart()
            {
                FogStart = this.FogStart,
                FogEnd = this.FogEnd,
                FogColor = this.FogColor,
            };
        }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.Projection { get { return Matrix.Identity; } set { } }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.World
        {
            get { return world; }
            set { world = value; DirtyMask |= fogVectorDirtyMask; }
        }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.View
        {
            get { return view; }
            set { view = value; DirtyMask |= fogVectorDirtyMask; }
        }

        [ContentSerializerIgnore]
        bool IEffectFog.FogEnabled { get { return true; } set { } }

        /// <summary>
        /// Sets a vector which can be dotted with the object space vertex position to compute fog amount.
        /// </summary>
        static void SetFogVector(ref Matrix worldView, float fogStart, float fogEnd, EffectParameter fogVectorParam)
        {
            if (fogStart == fogEnd)
            {
                // Degenerate case: force everything to 100% fogged if start and end are the same.
                fogVectorParam.SetValue(new Vector4(0, 0, 0, 1));
            }
            else
            {
                // We want to transform vertex positions into view space, take the resulting
                // Z value, then scale and offset according to the fog start/end distances.
                // Because we only care about the Z component, the shader can do all this
                // with a single dot product, using only the Z row of the world+view matrix.

                float scale = 1f / (fogStart - fogEnd);

                Vector4 fogVector = new Vector4();

                fogVector.X = worldView.M13 * scale;
                fogVector.Y = worldView.M23 * scale;
                fogVector.Z = worldView.M33 * scale;
                fogVector.W = (worldView.M43 + fogStart) * scale;

                fogVectorParam.SetValue(fogVector);
            }
        }
    }

#endif
}
