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
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName="Model Animation Processor - Nine")]
    public class ModelAnimationProcessor : ContentProcessor<NodeContent, Dictionary<string, AnimationClip>>
    {
        public override Dictionary<string, AnimationClip> Process(NodeContent input, ContentProcessorContext context)
        {
            // Flattern input node content
            List<NodeContent> nodes = new List<NodeContent>();
            
            FlattenNodeContent(input, nodes);


            // Process animations
            Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();

            ProcessAnimation(input, animations, nodes);


            // Sort keyframes
            foreach (AnimationClip clip in animations.Values)
            {
                clip.Keyframes.Sort(CompareKeyframeTimes);
            }

            return animations;
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
        static AnimationClip ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneMap)
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
    }
}
