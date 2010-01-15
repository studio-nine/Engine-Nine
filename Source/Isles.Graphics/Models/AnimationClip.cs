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
    /// An animation clip is the runtime equivalent of the
    /// Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single animation.
    /// </summary>
    public class AnimationClip
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


        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Isles.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static Dictionary<string, AnimationClip> FromModel(Model model)
        {
            object result = null;

            Dictionary<string, object> dictionary = model.Tag as Dictionary<string, object>;

            if (dictionary != null)
                dictionary.TryGetValue("AnimationData", out result);

            return result as Dictionary<string, AnimationClip>;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Isles.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static AnimationClip FromModel(Model model, string name)
        {
            AnimationClip result = null;

            Dictionary<string, AnimationClip> dictionary = FromModel(model);

            if (dictionary != null)
                dictionary.TryGetValue(name, out result);

            return result;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Isles.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static AnimationClip FromModel(Model model, int index)
        {
            AnimationClip result = null;

            Dictionary<string, AnimationClip> dictionary = FromModel(model);

            if (dictionary != null && dictionary.Count > 0)
            {
                foreach (string key in dictionary.Keys)
                {
                    if (index-- == 0)
                    {
                        dictionary.TryGetValue(key, out result);
                        break;
                    }
                }
            }

            return result;
        }
    }
}
