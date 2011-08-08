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

    internal class SkinTransformEffectPart : LinkedEffectPart, IEffectSkinned
    {
        EffectParameter bonesParameter;

        [ContentSerializer(Optional=true)]
        public int MaxBones { get; internal set; }

        [ContentSerializer(Optional = true)]
        public int WeightsPerVertex { get; internal set; }

        public SkinTransformEffectPart()
        {
            MaxBones = 72;
            WeightsPerVertex = 4;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new SkinTransformEffectPart();
        }
        
        bool IEffectSkinned.SkinningEnabled
        {
            get { return true; }
            set { }
        }

        /// <summary>
        /// Sets an array of skinning bone transform matrices.
        /// </summary>
        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            if ((boneTransforms == null) || (boneTransforms.Length == 0))
                throw new ArgumentNullException("boneTransforms");

            if (boneTransforms.Length > MaxBones)
                throw new ArgumentException();

            if (bonesParameter == null)
                bonesParameter = GetParameter("Bones");

            bonesParameter.SetValue(boneTransforms);
        }

        /// <summary>
        /// Gets a copy of the current skinning bone transform matrices.
        /// </summary>
        public Matrix[] GetBoneTransforms(int count)
        {
            if (count <= 0 || count > MaxBones)
                throw new ArgumentOutOfRangeException("count");

            if (bonesParameter == null)
                bonesParameter = GetParameter("Bones");

            Matrix[] bones = bonesParameter.GetValueMatrixArray(count);

            // Convert matrices from 43 to 44 format.
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].M44 = 1;
            }

            return bones;
        }
    }

#endif
}
