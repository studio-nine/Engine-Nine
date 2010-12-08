#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    public class TextureTransformEffectPart : LinkedEffectPart
    {
        private uint dirtyMask = 0;

        private Matrix textureTransform;
        private EffectParameter textureTransformParameter;
        private const uint textureTransformDirtyMask = 1 << 0;

        [ContentSerializer(Optional=true)]
        public Matrix Transform
        {
            get { return textureTransform; }
            set { textureTransform = value; dirtyMask |= textureTransformDirtyMask; }
        }

        public TextureTransformEffectPart()
        {
            Transform = Matrix.Identity;
        }
        
        protected internal override void OnApply()
        {
            if ((dirtyMask & textureTransformDirtyMask) != 0)
            {
                if (textureTransformParameter == null)
                    textureTransformParameter = GetParameter("TextureTransform");
                textureTransformParameter.SetValue(TextureTransform.ToArray(Transform));
                dirtyMask &= ~textureTransformDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new TextureTransformEffectPart();
        }
    }

#endif
}
