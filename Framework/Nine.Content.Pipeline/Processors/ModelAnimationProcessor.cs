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
using Nine.Animations;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    #region Keyframe
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    internal class Keyframe
    {
        /// <summary>
        /// Gets the index of the target bone that is animated by this keyframe.
        /// </summary>
        [ContentSerializer]
        public int Bone { get; private set; }


        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Time { get; private set; }


        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Matrix Transform { get; private set; }


        /// <summary>
        /// Constructs a new keyframe object.
        /// </summary>
        public Keyframe(int bone, TimeSpan time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private Keyframe() { }
    }
    #endregion

    #region AnimationClip
    /// <summary>
    /// It holds all the keyframes needed to describe a single animation.
    /// </summary>
    internal class AnimationClip
    {
        /// <summary>
        /// Gets the total length of the animation.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Duration { get; set; }


        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<Keyframe> Keyframes { get; private set; }


        /// <summary>
        /// Constructs a new animation clip object.
        /// </summary>
        public AnimationClip(TimeSpan duration, List<Keyframe> keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private AnimationClip() { }
    }
    #endregion

    #region ModelAnimationProcessor
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName="Model Animation - Engine Nine")]
    public class ModelAnimationProcessor : ContentProcessor<NodeContent, Dictionary<string, BoneAnimationClip>>
    {
        [DefaultValue(60)]
        [DisplayName("Animation Frame Rate")]
        public int FramesPerSecond { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationX { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationY { get; set; }

        [DefaultValue(0f)]
        public virtual float RotationZ { get; set; }

        [DefaultValue(1f)]
        public virtual float Scale { get; set; }

        /// <summary>
        /// Gets the skeleton data of this animation.
        /// </summary>
        internal ModelSkeletonData Skeleton { get; private set; }

        ContentProcessorContext context;

        /// <summary>
        /// Creates a new instance of ModelAnimationProcessor.
        /// </summary>
        public ModelAnimationProcessor()
        {
            FramesPerSecond = 60;
            Scale = 1;
        }

        public override Dictionary<string, BoneAnimationClip> Process(NodeContent input, ContentProcessorContext context)
        {
            ModelSkeletonProcessor skeletonProcessor = new ModelSkeletonProcessor();

            skeletonProcessor.RotationX = RotationX;
            skeletonProcessor.RotationY = RotationY;
            skeletonProcessor.RotationZ = RotationZ;
            skeletonProcessor.Scale = Scale;

            Skeleton = skeletonProcessor.Process(input, context);

            this.context = context;

            if (FramesPerSecond <= 0)
                throw new ArgumentOutOfRangeException("FramesPerSecond");

            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            // Flattern input node content
            List<NodeContent> nodes = new List<NodeContent>();
            
            FlattenNodeContent(input, nodes);

            // Process animations
            Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();

            ProcessAnimation(input, animations, nodes);

            if (animations.Count <= 0)
                return null;


            // Sort keyframes
            foreach (AnimationClip clip in animations.Values)
            {
                clip.Keyframes.Sort(CompareKeyframeTimes);
            }


            // Convert to BoneAnimationClip format
            Dictionary<string, BoneAnimationClip> boneAnimations = new Dictionary<string, BoneAnimationClip>();

            foreach (string key in animations.Keys)
            {
                boneAnimations.Add(key, ConvertAnimationClip(animations[key]));
            }

            ClampAnimationsAndApplyTransform(boneAnimations);

            return boneAnimations;
        }
        
        /// <summary>
        /// This method traverses the input bone hierarchy
        /// </summary>
        private void FlattenNodeContent(NodeContent input, List<NodeContent> nodes)
        {
            if (input != null)
            {
                nodes.Add(input);

                foreach (NodeContent child in input.Children)
                    FlattenNodeContent(child, nodes);
            }
        }

        /// <summary>
        /// Recursive traverses the input hierarchy, merge all animation clips
        /// </summary>
        private void ProcessAnimation(NodeContent input, Dictionary<string, AnimationClip> animations,
                                      IList<NodeContent> bones)
        {
            if (input != null)
            {
                if (input.Animations != null && input.Animations.Count > 0)
                {
                    Dictionary<string, AnimationClip> animSet = ProcessAnimations(input.Animations, bones);

                    foreach (KeyValuePair<string, AnimationClip> entry in animSet)
                    {
                        if (animations.ContainsKey(entry.Key))
                        {
                            animations[entry.Key].Keyframes.AddRange(entry.Value.Keyframes);
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
        

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Dictionary<string, AnimationClip> ProcessAnimations(AnimationContentDictionary animations, IList<NodeContent> bones)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                {
                    if (boneMap.ContainsKey(boneName))
                    {
                        throw new InvalidContentException(
                            "The input mesh contains multiple bones with the name: " + boneName);
                    }
                    boneMap.Add(boneName, i);
                }
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
                throw new InvalidContentException("Input file does not contain any animations.");
            }

            return animationClips;
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        static AnimationClip ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneMap)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    throw new InvalidContentException(string.Format(
                        "Found animation for bone '{0}', " +
                        "which is not part of the skeleton.", channel.Key));
                }

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time,
                                               keyframe.Transform));
                }
            }

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


        private BoneAnimationClip ConvertAnimationClip(AnimationClip animation)
        {            
            BoneAnimationClip boneAnimation = new BoneAnimationClip();
            boneAnimation.FramesPerSecond = FramesPerSecond;
            boneAnimation.TotalFrames = (int)Math.Round(animation.Duration.TotalSeconds * FramesPerSecond);


            int maxBones = 0;
            Dictionary<int, List<Keyframe>> keyframes = new Dictionary<int, List<Keyframe>>();
            foreach (Keyframe keyframe in animation.Keyframes)
            {
                if (keyframe.Bone > maxBones)
                    maxBones = keyframe.Bone;

                if (!keyframes.ContainsKey(keyframe.Bone))
                    keyframes.Add(keyframe.Bone, new List<Keyframe>());

                keyframes[keyframe.Bone].Add(keyframe);
            }


            boneAnimation.Transforms = new Matrix[maxBones + 1][];
            for (int i = 0; i <= maxBones; i++)
            {
                if (keyframes.ContainsKey(i))
                    boneAnimation.Transforms[i] = new Matrix[boneAnimation.TotalFrames];
            }


            TimeSpan time = TimeSpan.Zero;
            TimeSpan step = TimeSpan.FromSeconds(1.0 / FramesPerSecond);

            for (int frame = 0; frame < boneAnimation.TotalFrames; frame++)
            {
                foreach (int bone in keyframes.Keys)
                {
                    List<Keyframe> channel = keyframes[bone];
                    Matrix transform = Matrix.Identity;

                    if (time <= channel[0].Time)
                    {
                        transform = channel[0].Transform;
                    }
                    else if (time >= channel[channel.Count - 1].Time)
                    {
                        transform = channel[channel.Count - 1].Transform;
                    }
                    else
                    {
                        int k = 0;
                        while (channel[k].Time < time) k++;

                        double lerp = (channel[k].Time - time).TotalSeconds /
                                      (channel[k].Time - channel[k - 1].Time).TotalSeconds;

                        transform = LerpHelper.Slerp(
                            channel[k].Transform, 
                            channel[k - 1].Transform, (float)lerp);
                    }

                    boneAnimation.Transforms[bone][frame] = transform;
                }

                time += step;
            }

            FigureOutPreferredEnding(boneAnimation);

            return boneAnimation;
        }

        private void FigureOutPreferredEnding(BoneAnimationClip clip)
        {
            float maxDifference = 0;

            for (int bone = 0; bone < clip.Transforms.Length; bone++)
            {
                if (clip.Transforms[bone] == null)
                    continue;

                if (clip.TotalFrames <= 1)
                    continue;


                float a = (MatrixDifference(
                                    clip.Transforms[bone][0],
                                    clip.Transforms[bone][clip.TotalFrames - 1]) * 1000);
                float b = (MatrixDifference(
                                    clip.Transforms[bone][clip.TotalFrames - 1],
                                    clip.Transforms[bone][clip.TotalFrames - 2]) * 1000 + 1);

                float difference = a / b;

                if (difference > maxDifference)
                    maxDifference = difference;
            }

            float thresoldIdentical = 1000;
            float thresoldApproximate = 10000;

            if (maxDifference <= thresoldIdentical)
                clip.PreferredEnding = KeyframeEnding.Discard;
            else if (maxDifference <= thresoldApproximate)
                clip.PreferredEnding = KeyframeEnding.Wrap;
            else
                clip.PreferredEnding = KeyframeEnding.Clamp;
        }

        private float MatrixDifference(Matrix a, Matrix b)
        {
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.One;

            Vector3 a1 = Vector3.Transform(v1, a);
            Vector3 a2 = Vector3.Transform(v2, a);

            Vector3 b1 = Vector3.Transform(v1, b);
            Vector3 b2 = Vector3.Transform(v2, b);

            return Math.Abs((b1 - a1).Length() + (b2 - a2).Length());
        }

        private Matrix Transform(Matrix matrix)
        {
            return matrix * Matrix.CreateScale(Scale)
                          * Matrix.CreateRotationX(MathHelper.ToRadians(RotationX))
                          * Matrix.CreateRotationY(MathHelper.ToRadians(RotationY))
                          * Matrix.CreateRotationZ(MathHelper.ToRadians(RotationZ));
        }

        private void ClampAnimationsAndApplyTransform(Dictionary<string, BoneAnimationClip> animations)
        {
            if (Skeleton != null)
            {
                foreach (var animation in animations.Values)
                {
                    for (int i = 0; i < Skeleton.SkeletonRoot; i++)
                    {
                        if (i < animation.Transforms.Length)
                            animation.Transforms[i] = null;
                    }
                }
            }

            // Apply transform to the root bone.
            int root = Skeleton != null ? Skeleton.SkeletonRoot : 0;
            foreach (var animation in animations.Values)
            {
                if (animation.Transforms[root] != null)
                {
                    for (int i = 0; i < animation.Transforms[root].Length; i++)
                        animation.Transforms[root][i] = Transform(animation.Transforms[root][i]);
                }
            }
        }
    }
    #endregion
}
