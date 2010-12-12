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
    using Nine.Animations;

    /// <summary>
    /// Tag used by models.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ModelTag
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
        public Dictionary<string, BoneAnimationClip> Animations { get; internal set; }
    }

    /// <summary>
    /// Tag used by ModelMeshPart.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ModelMeshPartTag
    {
        internal ModelMeshPartTag() { }

        /// <summary>
        /// Gets the additional textures (E.g. normalmap) attached to the ModelMeshPart.
        /// </summary>
        [ContentSerializer()]
        public Dictionary<string, Texture> Textures { get; internal set; }
    }

    /// <summary>
    /// Contains extension methods to models.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModelExtensions
    {
        static List<string> EmptyStringCollection = new List<string>();

        /// <summary>
        /// Gets wether the specfied model has any skinning info attached to it.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static bool IsSkinned(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            return extensions != null && extensions.Skinning != null;
        }

        /// <summary>
        /// Gets the bone transformation matrices used to draw skinned models
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static Matrix[] GetBoneTransforms(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            if (extensions != null && extensions.Skinning != null)
            {
                return extensions.Skinning.GetBoneTransforms(model);
            }
            return null;
        }

        /// <summary>
        /// Gets the bone transformation matrices used to draw skinned models
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static Matrix[] GetBoneTransforms(this Model model, Matrix world)
        {
            ModelTag extensions = model.Tag as ModelTag;

            if (extensions != null && extensions.Skinning != null)
            {
                return extensions.Skinning.GetBoneTransforms(model, world);
            }
            return null;
        }

        /// <summary>
        /// Gets the bone transformation matrices used to draw skinned models
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static bool GetBoneTransforms(this Model model, Matrix world, Matrix[] boneTransforms)
        {
            ModelTag extensions = model.Tag as ModelTag;

            if (extensions != null && extensions.Skinning != null)
            {
                extensions.Skinning.GetBoneTransforms(model, world, boneTransforms);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static ICollection<string> GetAnimations(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            if (extensions != null && extensions.Animations != null)
            {
                return extensions.Animations.Keys;
            }
            return EmptyStringCollection;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static BoneAnimationClip GetAnimation(this Model model, string name)
        {
            ModelTag extensions = model.Tag as ModelTag;

            if (extensions != null && extensions.Animations != null)
            {
                BoneAnimationClip clip = null;
                extensions.Animations.TryGetValue(name, out clip);
                return clip;
            }

            return null;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static BoneAnimationClip GetAnimation(this Model model, int index)
        {
            BoneAnimationClip clip = null;

            ModelTag extensions = model.Tag as ModelTag;

            if (extensions != null && extensions.Animations != null &&
                extensions.Animations.Count > 0)
            {
                foreach (string key in extensions.Animations.Keys)
                {
                    if (index-- == 0)
                    {
                        extensions.Animations.TryGetValue(key, out clip);
                        break;
                    }
                }
            }

            return clip;
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

        /// <summary>
        /// Gets all the texture names attached to the model.
        /// </summary>
        public static ICollection<string> GetTextures(this ModelMeshPart part)
        {
            ModelMeshPartTag extensions = part.Tag as ModelMeshPartTag;

            if (extensions != null && extensions.Textures != null)
            {
                return extensions.Textures.Keys;
            }
            return EmptyStringCollection;
        }

        /// <summary>
        /// Gets the texture attached to the model with the specified name.
        /// </summary>
        public static Texture GetTexture(this ModelMeshPart part, string name)
        {
            Texture result;
            ModelMeshPartTag extensions = part.Tag as ModelMeshPartTag;

            if (extensions != null && extensions.Textures != null &&
                extensions.Textures.TryGetValue(name, out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Gets all the effects in the model.
        /// </summary>
        public static IEnumerable<Effect> GetEffects(this Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                    yield return part.Effect;
        }

        /// <summary>
        /// Converts all the effects of the Model to a new effect.
        /// Materials (Diffuse, Emissive, etc) and textures parameters are copied to the new effect.
        /// </summary>
        /// <remarks>
        /// This function requires the effect to be either build in effect or effects 
        /// that implements IEffectMaterial or IEffectTexture.
        /// </remarks>
        public static void ConvertEffectTo(this Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.ConvertEffectTo(effect.Clone());
                }
            }
        }

        /// <summary>
        /// Converts all the effects of the ModelMesh to a new effect.
        /// Materials (Diffuse, Emissive, etc) and textures parameters are copied to the new effect.
        /// </summary>
        /// <remarks>
        /// This function requires the effect to be either build in effect or effects 
        /// that implements IEffectMaterial or IEffectTexture.
        /// </remarks>
        public static void ConvertEffectTo(this ModelMesh mesh, Effect effect)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                part.ConvertEffectTo(effect.Clone());
            }
        }

        /// <summary>
        /// Converts the current effect of the ModelMeshPart to a new effect.
        /// Materials (Diffuse, Emissive, etc) and textures parameters are copied to the new effect.
        /// </summary>
        /// <remarks>
        /// This function requires the effect to be either build in effect or effects 
        /// that implements IEffectMaterial or IEffectTexture.
        /// </remarks>
        public static void ConvertEffectTo(this ModelMeshPart part, Effect effect)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");

            effect.CopyMaterialsFrom(part.Effect);

            if (effect.GetTexture() != null)
            {
                Texture2D texture = part.Effect.GetTexture();

                if (texture != null)
                    effect.SetTexture(texture);
            }

            IEffectTexture effectTexture = effect as IEffectTexture;
            if (effectTexture != null)
            {
                foreach (string texture in part.GetTextures())
                {
                    effectTexture.SetTexture(texture, part.GetTexture(texture));
                }
            }

            part.Effect = effect;
        }
    }
}
