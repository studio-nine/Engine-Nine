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

    [ContentSerializable]
    class BeginLightEffectPart : LinkedEffectPart, IEffectMatrices
    {
        private uint DirtyMask = 0;

        private Matrix view;
        private EffectParameter eyePositionParameter;
        private const uint eyePositionDirtyMask = 1 << 1;

        public BeginLightEffectPart()
        {
            eyePositionParameter = GetParameter("EyePosition");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & eyePositionDirtyMask) != 0)
            {
                if (eyePositionParameter != null)
                    eyePositionParameter.SetValue(Matrix.Invert(view).Translation);
                DirtyMask &= ~eyePositionDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new BeginLightEffectPart();
        }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.Projection { get { return Matrix.Identity; } set { } }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.World { get { return Matrix.Identity; } set { } }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.View
        {
            get { return view; }
            set { view = value; DirtyMask |= eyePositionDirtyMask; }
        }
    }

#endif
}
