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
    
    internal class TextureTransformEffectPart : LinkedEffectPart, IEffectTextureTransform
    {
        private uint DirtyMask = 0;

        private Matrix textureTransform;
        private EffectParameter textureTransformParameter;
        private const uint textureTransformDirtyMask = 1 << 0;

        public override bool IsMaterial { get { return true; } }

        [ContentSerializer(Optional=true)]
        public Matrix TextureTransform
        {
            get { return textureTransform; }
            set { textureTransform = value; DirtyMask |= textureTransformDirtyMask; }
        }

        public TextureTransformEffectPart()
        {
            TextureTransform = Matrix.Identity;
        }
        
        protected internal override void OnApply()
        {
            if ((DirtyMask & textureTransformDirtyMask) != 0)
            {
                if (textureTransformParameter == null)
                    textureTransformParameter = GetParameter("TextureTransform");
                textureTransformParameter.SetValue(Nine.Graphics.TextureTransform.ToArray(TextureTransform));
                DirtyMask &= ~textureTransformDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (TextureTransformEffectPart)part;
            effectPart.TextureTransform = TextureTransform;
        }        
        
        protected internal override LinkedEffectPart Clone()
        {
            return new TextureTransformEffectPart();
        }
    }

#endif
}
