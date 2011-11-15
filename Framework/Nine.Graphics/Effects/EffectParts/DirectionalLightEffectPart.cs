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
    
    internal class DirectionalLightEffectPart : LinkedEffectPart, IDirectionalLight
    {
        private uint DirtyMask = 0;
        
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
            set { direction = value; DirtyMask |= directionDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; DirtyMask |= diffuseColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; DirtyMask |= specularColorDirtyMask; }
        }

        public DirectionalLightEffectPart()
        {
            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;

            directionParameter = GetParameter("DirLightDirection");
            diffuseColorParameter = GetParameter("DirLightDiffuseColor");
            specularColorParameter = GetParameter("DirLightSpecularColor");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & directionDirtyMask) != 0)
            {
                if (directionParameter != null)
                    directionParameter.SetValue(direction);
                DirtyMask &= ~directionDirtyMask;
            }

            if ((DirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter != null)
                    diffuseColorParameter.SetValue(diffuseColor);
                DirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((DirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter != null)
                    specularColorParameter.SetValue(specularColor);
                DirtyMask &= ~specularColorDirtyMask;
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
