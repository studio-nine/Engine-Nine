#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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
        internal ModelTag() { }   
        
        /// <summary>
        /// Gets the skinning data attached to the model.
        /// </summary>
        [ContentSerializer()]
        public ModelSkinning Skinning { get; internal set; }

        /// <summary>
        /// Gets the collision data attached to the model.
        /// </summary>
        [ContentSerializer()]
        public ModelCollision Collision { get; internal set; }

        /// <summary>
        /// Gets the animation data attached to the model.
        /// </summary>
        [ContentSerializer()]
        public Dictionary<string, AnimationClip> Animations { get; internal set; }
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

        /// <summary>
        /// Gets wether the model contains the given point.
        /// </summary>
        public static bool Contains(this Model model, Matrix world, Vector3 point)
        {
            Vector3 local;
            ModelTag extensions = model.Tag as ModelTag;

            // Collision tree not found, use bounding sphere instead.
            if (extensions == null || extensions.Collision == null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    local = point - GetCollisionTreeTransform(mesh, world).Translation;
                    if (mesh.BoundingSphere.Contains(local) == ContainmentType.Contains)
                        return true;
                }

                return false;
            }

            // Detect using collision tree.
            local = point - GetCollisionTreeTransform(model.Meshes[0], world).Translation;
            return extensions.Collision.Contains(local);
        }

        /// <summary>
        /// Gets the nearest intersection point from the specifed picking ray.
        /// </summary>
        /// <returns>Distance to the start of the ray.</returns>
        public static float? Intersects(this Model model, Matrix world, Ray ray)
        {
            Ray local;
            ModelTag extensions = model.Tag as ModelTag;

            // Collision tree not found, use bounding sphere instead.
            if (extensions == null || extensions.Collision == null)
            {
                float? result = null;

                foreach (ModelMesh mesh in model.Meshes)
                {
                    local = ray.Transform(Matrix.Invert(GetCollisionTreeTransform(mesh, world)));
                    float? current = mesh.BoundingSphere.Intersects(local);

                    if (result == null)
                        result = current;
                    else if (current.Value < result.Value)
                        result = current.Value;
                }

                return result;
            }

            // Detect using collision tree.
            local = ray.Transform(Matrix.Invert(GetCollisionTreeTransform(model.Meshes[0], world)));
            return extensions.Collision.Intersects(local);
        }
        
        private static Matrix GetCollisionTreeTransform(ModelMesh mesh, Matrix world)
        {
            // In case animation changes the tranform of the node containning the model,
            // gets the ambsolute transform of the first mesh before transform to world
            // space.
            ModelBone currentBone = mesh.ParentBone;
            Matrix ambsoluteTransform = currentBone.Transform;

            while (currentBone.Parent != null)
            {
                currentBone = currentBone.Parent;
                ambsoluteTransform = currentBone.Transform * ambsoluteTransform;
            }
            
            return ambsoluteTransform * world;
        }
    }
}
