#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics.Models
{
    /// <summary>
    /// Combines all the data needed to render and animate a skinned object.
    /// This is typically stored in the Tag property of the Model being animated.
    /// </summary>
    public class ModelSkinning
    {
        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> InverseBindPose { get; private set; }
        
        
        /// <summary>
        /// Index of the skeleton root on the parent mesh bone collection.
        /// </summary>
        [ContentSerializer]
        public int SkeletonIndex { get; private set; }


        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        public ModelSkinning(List<Matrix> inverseBindPose, int skeleton)
        {
            if (skeleton < 0 || inverseBindPose == null || inverseBindPose.Count <= 0)
                throw new ArgumentException("Error creating skinner.");

            InverseBindPose = inverseBindPose;
            SkeletonIndex = skeleton;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private ModelSkinning() { }

        
        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        /// <returns>
        /// A matrix array used to draw skinned meshes.
        /// </returns>
        /// <remarks>
        /// Whenever the bone or skeleton changes, you should re-skin the model.
        /// </remarks>
        public Matrix[] GetBoneTransforms(Model model)
        {
            return GetBoneTransforms(model, Matrix.Identity);
        }

        /// <summary>
        /// Skin the target model based on the current state of model bone transforms.
        /// </summary>
        /// <param name="world">
        /// A world matrix that will be applied to the result bone transforms.
        /// </param>
        /// <returns>
        /// A matrix array used to draw skinned meshes.
        /// </returns>
        /// <remarks>
        /// Whenever the bone or skeleton changes, you should re-skin the model.
        /// </remarks>
        public Matrix[] GetBoneTransforms(Model model, Matrix world)
        {
            Matrix[] skin = new Matrix[InverseBindPose.Count];

            if (bones == null || bones.Length < model.Bones.Count)
                bones = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(bones);

            for (int i = 0; i < InverseBindPose.Count; i++)
            {
                // Apply inverse bind pose
                skin[i] = InverseBindPose[i] * bones[SkeletonIndex + i] * world;
            }

            return skin;
        }

        static Matrix[] bones = null;
    }
}
