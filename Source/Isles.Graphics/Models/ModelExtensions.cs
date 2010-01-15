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
    public static class ModelExtensions
    {
        /// <summary>
        /// Gets the skinning data associated with the specified model.
        /// Works with models that are processed by Isles.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static ModelSkinning GetSkinning(Model model)
        {
            object result = null;

            Dictionary<string, object> dictionary = model.Tag as Dictionary<string, object>;

            if (dictionary != null)
                dictionary.TryGetValue("SkinningData", out result);

            return result as ModelSkinning;
        }


        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Isles.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static Dictionary<string, AnimationClip> GetAnimations(Model model)
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
        public static AnimationClip GetAnimation(Model model, string name)
        {
            AnimationClip result = null;

            Dictionary<string, AnimationClip> dictionary = GetAnimations(model);

            if (dictionary != null)
                dictionary.TryGetValue(name, out result);

            return result;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Isles.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static AnimationClip GetAnimation(Model model, int index)
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
