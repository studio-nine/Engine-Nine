﻿#region Copyright 2009 - 2010 (c) Engine Nine
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
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    public class DirectionalLightEffectPart : LinkedEffectPart
    {
        private uint dirtyMask = 0;
        
        private Vector3 direction;
        private EffectParameter directionParameter;
        private const uint directionDirtyMask = 1 << 0;

        private Vector3 diffuseColor;
        private EffectParameter diffuseColorParameter;
        private const uint diffuseColorDirtyMask = 1 << 1;

        private Vector3 specularColor;
        private EffectParameter specularColorParameter;
        private const uint specularColorDirtyMask = 1 << 2;
        
        [ContentSerializer(Optional = true)]
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; dirtyMask |= directionDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; dirtyMask |= diffuseColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; dirtyMask |= specularColorDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((dirtyMask & directionDirtyMask) != 0)
            {
                if (directionParameter == null)
                    directionParameter = GetParameter("DirLightDirection");
                directionParameter.SetValue(direction);
                dirtyMask &= ~directionDirtyMask;
            }

            if ((dirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter == null)
                    diffuseColorParameter = GetParameter("DirLightDiffuseColor");
                diffuseColorParameter.SetValue(diffuseColor);
                dirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((dirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter == null)
                    specularColorParameter = GetParameter("DirLightSpecularColor");
                specularColorParameter.SetValue(specularColor);
                dirtyMask &= ~specularColorDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new DirectionalLightEffectPart()
            {
                Direction = this.Direction,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
            };
        }
    }

#endif
}