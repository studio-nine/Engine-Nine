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
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Tag used by models.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ModelTag
    {
        public ModelSkinning Skinning { get; set; }
        public Dictionary<string, AnimationClip> Animations { get; set; }
    }

    /// <summary>
    /// Contains extension methods to models.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModelExtensions
    {
        /// <summary>
        /// Gets the skinning data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static ModelSkinning GetSkinning(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            return extensions != null ? extensions.Skinning : null;
        }


        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static Dictionary<string, AnimationClip> GetAnimations(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            return extensions != null ? extensions.Animations : null;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static AnimationClip GetAnimation(this Model model, string name)
        {
            AnimationClip result = null;

            Dictionary<string, AnimationClip> dictionary = GetAnimations(model);

            if (dictionary != null)
                dictionary.TryGetValue(name, out result);

            return result;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static AnimationClip GetAnimation(this Model model, int index)
        {
            AnimationClip result = null;

            Dictionary<string, AnimationClip> dictionary = GetAnimations(model);

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
