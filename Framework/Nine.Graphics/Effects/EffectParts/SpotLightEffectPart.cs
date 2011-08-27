#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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

    internal class SpotLightEffectPart : LinkedEffectPart, ISpotLight
    {
        private uint DirtyMask = 0;
        
        private Vector3 position;
        private EffectParameter positionParameter;
        private const uint positionDirtyMask = 1 << 0;
        
        private Vector3 diffuseColor;
        private EffectParameter diffuseColorParameter;
        private const uint diffuseColorDirtyMask = 1 << 1;

        private Vector3 specularColor;
        private EffectParameter specularColorParameter;
        private const uint specularColorDirtyMask = 1 << 2;

        private float range;
        private EffectParameter rangeParameter;
        private const uint rangeDirtyMask = 1 << 3;

        private float attenuation;
        private EffectParameter attenuationParameter;
        private const uint attenuationDirtyMask = 1 << 4;

        private float innerAngle;
        private EffectParameter innerAngleParameter;
        private const uint innerAngleDirtyMask = 1 << 5;

        private float outerAngle;
        private EffectParameter outerAngleParameter;
        private const uint outerAngleDirtyMask = 1 << 6;

        private float falloff;
        private EffectParameter falloffParameter;
        private const uint falloffDirtyMask = 1 << 7;

        private Vector3 direction;
        private EffectParameter directionParameter;
        private const uint directionDirtyMask = 1 << 8;
        
        [ContentSerializer(Optional = true)]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; DirtyMask |= positionDirtyMask; }
        }

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

        [ContentSerializer(Optional = true)]
        public float Range
        {
            get { return range; }
            set { range = value; DirtyMask |= rangeDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; DirtyMask |= attenuationDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float InnerAngle
        {
            get { return innerAngle; }
            set { innerAngle = value; DirtyMask |= innerAngleDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float OuterAngle
        {
            get { return outerAngle; }
            set { outerAngle = value; DirtyMask |= outerAngleDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Falloff
        {
            get { return falloff; }
            set { falloff = value; DirtyMask |= falloffDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & positionDirtyMask) != 0)
            {
                if (positionParameter == null)
                    positionParameter = GetParameter("SpotLightPosition");
                positionParameter.SetValue(position);
                DirtyMask &= ~positionDirtyMask;
            }

            if ((DirtyMask & directionDirtyMask) != 0)
            {
                if (directionParameter == null)
                    directionParameter = GetParameter("SpotLightDirection");
                directionParameter.SetValue(direction);
                DirtyMask &= ~directionDirtyMask;
            }

            if ((DirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter == null)
                    diffuseColorParameter = GetParameter("SpotLightDiffuseColor");
                diffuseColorParameter.SetValue(diffuseColor);
                DirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((DirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter == null)
                    specularColorParameter = GetParameter("SpotLightSpecularColor");
                specularColorParameter.SetValue(specularColor);
                DirtyMask &= ~specularColorDirtyMask;
            }

            if ((DirtyMask & rangeDirtyMask) != 0)
            {
                if (rangeParameter == null)
                    rangeParameter = GetParameter("Range");
                rangeParameter.SetValue(range);
                DirtyMask &= ~rangeDirtyMask;
            }

            if ((DirtyMask & attenuationDirtyMask) != 0)
            {
                if (attenuationParameter == null)
                    attenuationParameter = GetParameter("Attenuation");
                attenuationParameter.SetValue(attenuation);
                DirtyMask &= ~attenuationDirtyMask;
            }

            if ((DirtyMask & innerAngleDirtyMask) != 0)
            {
                if (innerAngleParameter == null)
                    innerAngleParameter = GetParameter("InnerAngle");
                innerAngleParameter.SetValue((float)Math.Cos(innerAngle * 0.5));
                DirtyMask &= ~innerAngleDirtyMask;
            }

            if ((DirtyMask & outerAngleDirtyMask) != 0)
            {
                if (outerAngleParameter == null)
                    outerAngleParameter = GetParameter("OuterAngle");
                outerAngleParameter.SetValue((float)Math.Cos(outerAngle * 0.5));
                DirtyMask &= ~outerAngleDirtyMask;
            }

            if ((DirtyMask & falloffDirtyMask) != 0)
            {
                if (falloffParameter == null)
                    falloffParameter = GetParameter("Falloff");
                falloffParameter.SetValue(falloff);
                DirtyMask &= ~falloffDirtyMask;
            }
        }

        public SpotLightEffectPart()
        {
            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
            Range = 100;
            Attenuation = MathHelper.E;
            InnerAngle = MathHelper.PiOver4;
            OuterAngle = MathHelper.PiOver2;
            Falloff = 1;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new SpotLightEffectPart()
            {
                Position = this.Position,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                Range = this.Range,
                Attenuation = this.Attenuation,
                InnerAngle = this.InnerAngle,
                OuterAngle = this.OuterAngle,
                Falloff = this.Falloff,
            };
        }
    }

#endif
}
