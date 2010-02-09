#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Isles.Graphics.Models;
#endregion

namespace Isles.Pipeline.Processors
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName="Extended Model Processor - Isles")]
    public class ExtendedModelProcessor : ModelProcessor
    {
        // Maximum number of bone matrices we can render using shader 2.0
        // in a single pass. If you change this, update SkinnedModel.fx to match.
        const int MaxBones = 59;

        [DefaultValue("_n")]
        public string NormalTextureExtension { get; set; }


        public ExtendedModelProcessor()
        {
            NormalTextureExtension = "_n";
        }

        
        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ModelContent model;
            Dictionary<string, AnimationClip> animationClips;
            ModelTag tag = new ModelTag();

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            //System.Diagnostics.Debugger.Launch();

            if (skeleton == null)
            {
                // Not a skinned mesh
                model = base.Process(input, context);

                animationClips = ProcessAnimations(input);

                tag.Animations = animationClips;

                model.Tag = tag;

                AddNormalTextureToTag(model);
                return model;
            }

            ValidateMesh(input, context, null);

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            int skeletonIndex = FindSkeletonIndex(input, skeleton, 0);

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

            // Convert animation data to our runtime format.
            animationClips = ProcessAnimations(input);//skeleton.Animations, BonesToNodes(bones));

            // Chain to the base ModelProcessor class so it can convert the model data.
            model = base.Process(input, context);

            // Store our custom animation data in the Tag property of the model.
            tag.Skinning = new ModelSkinning(inverseBindPose, skeletonIndex);
            tag.Animations = animationClips;

            model.Tag = tag;

            // Store normal texture
            AddNormalTextureToTag(model);

            return model;
        }

        private int FindSkeletonIndex(NodeContent input, BoneContent skeleton, int n)
        {
            if (input == skeleton)
                return n;

            int result;

            foreach (NodeContent child in input.Children)
                if ((result = FindSkeletonIndex(child, skeleton, ++n)) > 0)
                    return result;

            return -1;
        }

        private Dictionary<string, AnimationClip> ProcessAnimations(NodeContent input)
        {
            IList<NodeContent> bones = FlattenNodes(input);

            Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();

            ProcessAnimation(input, animations, bones);

            return animations;
        }

        private void ProcessAnimation(NodeContent input, Dictionary<string, AnimationClip> animations,
                                      IList<NodeContent> bones)
        {
            if (input != null)
            {
                if (input.Animations != null && input.Animations.Count > 0)
                {
                    Dictionary<string, AnimationClip> animSet = ProcessAnimations(input.Animations,
                                                                                  bones);
                    foreach (KeyValuePair<string, AnimationClip> entry in animSet)
                    {
                        if (animations.ContainsKey(entry.Key))
                        {
                            foreach (Keyframe frame in entry.Value.Keyframes)
                                animations[entry.Key].Keyframes.Add(frame);
                            (animations[entry.Key].Keyframes as List<Keyframe>).Sort(CompareKeyframeTimes);
                        }
                        else
                        {
                            animations.Add(entry.Key, entry.Value);
                        }
                    }
                }

                foreach (NodeContent child in input.Children)
                    ProcessAnimation(child, animations, bones);
            }
        }

        private IList<NodeContent> FlattenNodes(NodeContent input)
        {
            List<NodeContent> nodes = new List<NodeContent>();

            FlattenNodes(input, nodes);

            return nodes;
        }

        private void FlattenNodes(NodeContent input, List<NodeContent> nodes)
        {
            if (input != null)
            {
                nodes.Add(input);

                foreach (NodeContent child in input.Children)
                    FlattenNodes(child, nodes);
            }
        }

        IList<NodeContent> BonesToNodes(IList<BoneContent> bones)
        {
            List<NodeContent> nodes = new List<NodeContent>(bones.Count);

            foreach (BoneContent bone in bones)
                nodes.Add(bone);

            return nodes;
        }


        void AddNormalTextureToTag(ModelContent model)
        {
            foreach (ModelMeshContent mesh in model.Meshes)
                foreach (ModelMeshPartContent part in mesh.MeshParts)
                {
                    ExternalReference<TextureContent> value;
                    part.Material.Textures.TryGetValue("NormalTexture", out value);
                    part.Tag = value;
                }
        }

        /// <summary>
        /// Changes all the materials to use our skinned model effect.
        /// </summary>
        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                        ContentProcessorContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            BasicMaterialContent basicMaterial = material as BasicMaterialContent;

            if (basicMaterial == null)
            {
                throw new InvalidContentException(string.Format(
                    "ExtendedModelProcessor only supports BasicMaterialContent, " +
                    "but input mesh uses {0}.", material.GetType()));
            }

            // Compile the corresponding normal texture
            if (basicMaterial.Texture == null)
            {
                context.Logger.LogWarning(null, null, "No texture found");
                return basicMaterial;
            }

            string textureFilename = basicMaterial.Texture.Filename;
            string normalTextureFilename =
                Path.GetDirectoryName(textureFilename) + "\\" +
                Path.GetFileNameWithoutExtension(textureFilename) + NormalTextureExtension +
                Path.GetExtension(textureFilename);

            // Checks if the normal texture exists
            if (File.Exists(normalTextureFilename))
            {
                ExternalReference<Texture2DContent> normalTexture =
                    new ExternalReference<Texture2DContent>(normalTextureFilename);

                // Store the normal map in the opaque data of the corresponding material
                basicMaterial.Textures.Add("NormalTexture",
                    context.BuildAsset<Texture2DContent, TextureContent>(
                        normalTexture, typeof(NormalTextureProcessor).Name));
            }
            else if (GenerateTangentFrames)
            {
                context.Logger.LogWarning(null, null,
                    "Missing normal texture: {0}", normalTextureFilename);
            }

            // Chain to the base ModelProcessor converter.
            return base.ConvertMaterial(basicMaterial, context);
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Dictionary<string, AnimationClip> ProcessAnimations(
            AnimationContentDictionary animations, IList<NodeContent> bones)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = new Dictionary<string, AnimationClip>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap);

                animationClips.Add(animation.Key, processed);
            }

            if (animationClips.Count == 0)
            {
                throw new InvalidContentException(
                            "Input file does not contain any animations.");
            }

            return animationClips;
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        static AnimationClip ProcessAnimation(AnimationContent animation,
                                              Dictionary<string, int> boneMap)
        {
            //System.Diagnostics.Debugger.Launch();
            List<Keyframe> keyframes = new List<Keyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in
                animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    //throw new InvalidContentException(string.Format(
                    //    "Found animation for bone '{0}', " +
                    //    "which is not part of the skeleton.", channel.Key));
                    continue;
                }

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time,
                                               keyframe.Transform));
                }
            }

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new AnimationClip(animation.Duration, keyframes);
        }


        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(Keyframe a, Keyframe b)
        {
            return a.Time.CompareTo(b.Time);
        }


        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context,
                                 string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. SkinnedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    //context.Logger.LogWarning(null, null,
                    //    "Mesh {0} has no skinning information, so it has been deleted.",
                    //    mesh.Name);

                    //mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
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

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }
    }
}
