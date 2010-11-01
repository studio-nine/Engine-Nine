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
using System.IO;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Custom processor that extract skinning info from a skinned model.
    /// </summary>
    /// <remarks>
    /// This processor is used by ExtendedModelProcessor,
    /// There is no need to expose it to the xna build.
    ///</remarks>
    // [ContentProcessor(DisplayName="Model Skinning Processor - Nine")]
    public class ModelSkinningProcessor : ContentProcessor<NodeContent, ModelSkinning>
    {
        // Maximum number of bone matrices we can render using shader 2.0
        // in a single pass. If you change this, update SkinnedModel.fx to match.
        const int MaxBones = 59;


        public override ModelSkinning Process(NodeContent input, ContentProcessorContext context)
        {
            // Check if the input model is a skinned model.
            if (!IsSkinned(input))
                return null;


            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                return null;


            // Find the index of the first bone content.
            int skeletonIndex = 0;

            if (!FindSkeletonIndex(input, skeleton, ref skeletonIndex))
                return null;
            

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);
            
            if (bones.Count > MaxBones)
            {
                throw new InvalidContentException(string.Format(
                    "Skeleton has {0} bones, but the maximum supported is {1}.",
                    bones.Count, MaxBones));
            }

            List<Matrix> inverseBindPose = new List<Matrix>();

            foreach (BoneContent bone in bones)
            {
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
            }
            
            // Store our custom animation data in the Tag property of the model.
            return new ModelSkinning(inverseBindPose, skeletonIndex);
        }

        /// <summary>
        /// Finds the index of the first node content that is a bone content.
        /// </summary>
        private bool FindSkeletonIndex(NodeContent input, BoneContent skeleton, ref int n)
        {
            if (input == skeleton)
                return true;

            foreach (NodeContent child in input.Children)
            {
                n++;
                if (FindSkeletonIndex(child, skeleton, ref n))
                {
                    return true;
                }
            }

            return false;
        }
                
        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks whether a mesh is a skinned mesh
        /// </summary>
        static bool IsSkinned(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null && MeshHasSkinning(mesh))
                return true;

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                if (IsSkinned(child))
                    return true;

            return false;
        }
    }
}
