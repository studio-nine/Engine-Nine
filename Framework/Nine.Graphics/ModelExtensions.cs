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
        internal ModelSkeletonData Skeleton { get; set; }

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
        /// Gets the bounding box of this ModelMeshPart.
        /// </summary>
        [ContentSerializer()]
        public BoundingBox BoundingBox { get; internal set; }

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
        /// Gets whether the specified model has any skinning info attached to it.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static bool IsSkinned(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            return extensions != null && extensions.Skeleton != null;
        }

        internal static ModelSkeletonData GetSkeletonData(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            return extensions != null ? extensions.Skeleton : null;
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
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public static Matrix GetAbsoluteBoneTransform(this Model model, int boneIndex)
        {
            ModelBone bone = model.Bones[boneIndex];
            Matrix absoluteTransform = bone.Transform;
            
            while (bone.Parent != null)
            {
                bone = bone.Parent;
                absoluteTransform = absoluteTransform * bone.Transform;
            }

            return absoluteTransform;
        }

        /// <summary>
        /// Gets the aboslute transform of the specified bone.
        /// </summary>
        public static Matrix GetAbsoluteBoneTransform(this Model model, string boneName)
        {
            return GetAbsoluteBoneTransform(model, model.Bones[boneName].Index);
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
                    local = point - GetAbsoluteTransform(mesh, world).Translation;
                    if (mesh.BoundingSphere.Contains(local) == ContainmentType.Contains)
                        return true;
                }

                return false;
            }

            // Detect using collision tree.
            local = point - GetAbsoluteTransform(model.Meshes[0], world).Translation;
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
                    local = ray.Transform(Matrix.Invert(GetAbsoluteTransform(mesh, world)));
                    float? current = mesh.BoundingSphere.Intersects(local);

                    if (result == null)
                        result = current;
                    else if (current.Value < result.Value)
                        result = current.Value;
                }
                
                return result;
            }

            // Detect using collision tree.
            local = ray.Transform(Matrix.Invert(GetAbsoluteTransform(model.Meshes[0], world)));
            return extensions.Collision.Intersects(local);
        }

        internal static Matrix GetAbsoluteTransform(this ModelMesh mesh)
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

            return ambsoluteTransform;
        }
        
        internal static Matrix GetAbsoluteTransform(this ModelMesh mesh, Matrix world)
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
        
        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        public static BoundingBox ComputeBoundingBox(this Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            // Try to use collision tree.
            if (extensions != null && extensions.Collision != null)
            {
                return extensions.Collision.CollisionTree.Bounds;
            }

            // Try to use vertices of the mesh
            BoundingBox result = new BoundingBox();
            if (ComputeBoundingBoxFromVertices(model, out result))
                return result;

            // Now use bounding spheres
            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingBox box = BoundingBox.CreateFromSphere(
                    mesh.BoundingSphere.Transform(GetAbsoluteTransform(mesh, Matrix.Identity)));
                result = BoundingBox.CreateMerged(result, box);
            }

            return result;
        }
        
        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        private static bool ComputeBoundingBoxFromVertices(this Model model, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox(Vector3.Zero, Vector3.Zero);
            if (null == model || model.Meshes.Count <= 0)
                return false;

            const float FloatMax = float.MaxValue;

            // Compute bounding box
            Vector3 min = new Vector3(FloatMax, FloatMax, FloatMax);
            Vector3 max = new Vector3(-FloatMax, -FloatMax, -FloatMax);

            Matrix[] bones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones);

            List<VertexBuffer> usedBuffers = new List<VertexBuffer>();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.VertexBuffer.BufferUsage == BufferUsage.WriteOnly)
                        return false;

                    if (usedBuffers.Contains(part.VertexBuffer))
                        continue;

                    int stride = part.VertexBuffer.VertexDeclaration.VertexStride;
                    int elementCount = part.VertexBuffer.VertexCount;
                    Vector3[] vertices = new Vector3[elementCount];
                    part.VertexBuffer.GetData<Vector3>(0, vertices, 0, elementCount, stride);

                    foreach (Vector3 vertex in vertices)
                    {
                        // Transform vertex
                        Vector3 v = Vector3.Transform(vertex, bones[mesh.ParentBone.Index]);

                        if (v.X < min.X)
                            min.X = v.X;
                        if (v.X > max.X)
                            max.X = v.X;

                        if (v.Y < min.Y)
                            min.Y = v.Y;
                        if (v.Y > max.Y)
                            max.Y = v.Y;

                        if (v.Z < min.Z)
                            min.Z = v.Z;
                        if (v.Z > max.Z)
                            max.Z = v.Z;
                    }

                    usedBuffers.Add(part.VertexBuffer);
                }
            }

            boundingBox = new BoundingBox(min, max);
            return true;
        }


        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        public static BoundingBox ComputeBoundingBox(this Model model, ModelMesh mesh, ModelMeshPart part)
        {
            // Try to use vertices of the mesh
            BoundingBox result = new BoundingBox();
            if (ComputeBoundingBoxFromVertices(model, mesh, part, out result))
                return result;

            // Now use bounding spheres
            return BoundingBox.CreateFromSphere(mesh.BoundingSphere);
        }

        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        private static bool ComputeBoundingBoxFromVertices(this Model model, ModelMesh mesh, ModelMeshPart part, out BoundingBox boundingBox)
        {
            boundingBox = new BoundingBox();
            if (null == model || model.Meshes.Count <= 0)
                return false;

            if (part.VertexBuffer.BufferUsage == BufferUsage.WriteOnly ||
                part.IndexBuffer.BufferUsage == BufferUsage.WriteOnly)
                return false;

            const float FloatMax = float.MaxValue;

            // Compute bounding box
            Vector3 min = new Vector3(FloatMax, FloatMax, FloatMax);
            Vector3 max = new Vector3(-FloatMax, -FloatMax, -FloatMax);

            int stride = part.VertexBuffer.VertexDeclaration.VertexStride;
            int elementCount = part.VertexBuffer.VertexCount;
            int indexCount = part.IndexBuffer.IndexCount;
            Vector3[] vertices = new Vector3[elementCount];
            ushort[] indices = new ushort[indexCount];
            
            part.VertexBuffer.GetData<Vector3>(0, vertices, 0, elementCount, stride);
            part.IndexBuffer.GetData<ushort>(0, indices, part.StartIndex, part.PrimitiveCount * 3);

            for (int i = part.StartIndex; i < part.StartIndex + part.PrimitiveCount * 3; i++)
            {
                Vector3 v = vertices[indices[i]];

                if (v.X < min.X)
                    min.X = v.X;
                if (v.X > max.X)
                    max.X = v.X;

                if (v.Y < min.Y)
                    min.Y = v.Y;
                if (v.Y > max.Y)
                    max.Y = v.Y;

                if (v.Z < min.Z)
                    min.Z = v.Z;
                if (v.Z > max.Z)
                    max.Z = v.Z;
            }

            boundingBox = new BoundingBox(min, max);
            return true;
        }
    }
}
