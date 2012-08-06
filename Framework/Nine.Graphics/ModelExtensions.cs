namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;


    /// <summary>
    /// Tag used by models.
    /// </summary>
    [ContentSerializable]
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
    [ContentSerializable]
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
        public Dictionary<TextureUsage, Texture> Textures { get; internal set; }
    }

    /// <summary>
    /// Contains extension methods to models.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModelExtensions
    {
        static List<string> EmptyStringCollection = new List<string>();
        static List<TextureUsage> EmptyTextureNamesCollection = new List<TextureUsage>();

        /// <summary>
        /// Gets whether the specified model has any skinning info attached to it.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static bool IsSkinned(this Microsoft.Xna.Framework.Graphics.Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            return extensions != null && extensions.Skeleton != null;
        }

        internal static ModelSkeletonData GetSkeletonData(this Microsoft.Xna.Framework.Graphics.Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            return extensions != null ? extensions.Skeleton : null;
        }

        /// <summary>
        /// Gets the animation data associated with the specified model.
        /// Works with models that are processed by Nine.Pipeline.Processors.ExtendedModelProcessor.
        /// </summary>
        public static ICollection<string> GetAnimations(this Microsoft.Xna.Framework.Graphics.Model model)
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
        public static BoneAnimationClip GetAnimation(this Microsoft.Xna.Framework.Graphics.Model model, string name)
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
        public static BoneAnimationClip GetAnimation(this Microsoft.Xna.Framework.Graphics.Model model, int index)
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
        public static Matrix GetAbsoluteBoneTransform(this Microsoft.Xna.Framework.Graphics.Model model, int boneIndex)
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
        public static Matrix GetAbsoluteBoneTransform(this Microsoft.Xna.Framework.Graphics.Model model, string boneName)
        {
            return GetAbsoluteBoneTransform(model, model.Bones[boneName].Index);
        }

        /// <summary>
        /// Gets wether the model contains the given point.
        /// </summary>
        public static bool Contains(this Microsoft.Xna.Framework.Graphics.Model model, Matrix world, Vector3 point)
        {
            Vector3 local;
            ModelTag extensions = model.Tag as ModelTag;

            // Collision tree not found, use bounding sphere instead.
            if (extensions == null || extensions.Collision == null)
            {
                foreach (var mesh in model.Meshes)
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
        public static float? Intersects(this Microsoft.Xna.Framework.Graphics.Model model, Matrix world, Ray ray)
        {
            Ray local;
            ModelTag extensions = model.Tag as ModelTag;

            // Collision tree not found, use bounding sphere instead.
            if (extensions == null || extensions.Collision == null)
            {
                float? result = null;

                foreach (var mesh in model.Meshes)
                {
                    local = ray.Transform(Matrix.Invert(GetAbsoluteTransform(mesh, world)));
                    float? current = mesh.BoundingSphere.Intersects(local);

                    if (result == null)
                        result = current;
                    else if (current.HasValue && current.Value < result.Value)
                        result = current.Value;
                }
                
                return result;
            }

            // Detect using collision tree.

            local = ray.Transform(Matrix.Invert(GetAbsoluteTransform(model.Meshes[0], world)));
            return extensions.Collision.Intersects(local);
        }

        internal static Matrix GetAbsoluteTransform(this Microsoft.Xna.Framework.Graphics.ModelMesh mesh)
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

        internal static Matrix GetAbsoluteTransform(this Microsoft.Xna.Framework.Graphics.ModelMesh mesh, Matrix world)
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
        public static ICollection<TextureUsage> GetTextures(this ModelMeshPart part)
        {
            ModelMeshPartTag extensions = part.Tag as ModelMeshPartTag;

            if (extensions != null && extensions.Textures != null)
            {
                return extensions.Textures.Keys;
            }
            return EmptyTextureNamesCollection;
        }

        /// <summary>
        /// Gets the texture attached to the model with the specified name.
        /// </summary>
        public static Texture GetTexture(this ModelMeshPart part, TextureUsage usage)
        {
            Texture result;
            ModelMeshPartTag extensions = part.Tag as ModelMeshPartTag;

            if (extensions != null && extensions.Textures != null &&
                extensions.Textures.TryGetValue(usage, out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Gets all the effects in the model.
        /// </summary>
        public static IEnumerable<Effect> GetEffects(this Microsoft.Xna.Framework.Graphics.Model model)
        {
            foreach (var mesh in model.Meshes)
                foreach (var part in mesh.MeshParts)
                    yield return part.Effect;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Converts all the effects of the Model to a new effect.
        /// Materials (Diffuse, Emissive, etc) and textures parameters are copied to the new effect.
        /// </summary>
        /// <remarks>
        /// This function requires the effect to be either build in effect or effects 
        /// that implements IEffectMaterial or IEffectTexture.
        /// </remarks>
        public static void ConvertEffectTo(this Microsoft.Xna.Framework.Graphics.Model model, Effect effect)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
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
        public static void ConvertEffectTo(this Microsoft.Xna.Framework.Graphics.ModelMesh mesh, Effect effect)
        {
            foreach (var part in mesh.MeshParts)
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
            ConvertEffectTo(part, effect, false, false);
        }

        /// <summary>
        /// Converts the current effect of the ModelMeshPart to a new effect.
        /// Materials (Diffuse, Emissive, etc) and textures parameters are copied to the new effect.
        /// </summary>
        internal static void ConvertEffectTo(this ModelMeshPart part, Effect effect, bool overrideMaterials,
                                                                                     bool overrideTextures)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");

            if (!overrideMaterials)
            {
                effect.CopyMaterialsFrom(part.Effect);
            }

            if (!overrideTextures)
            {
                effect.SetTexture(part.Effect.GetTexture());
            }
            part.Effect = effect;
        }
#endif
        
        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        public static BoundingBox ComputeBoundingBox(this Microsoft.Xna.Framework.Graphics.Model model)
        {
            ModelTag extensions = model.Tag as ModelTag;

            // Try to use collision tree.
            if (extensions != null && extensions.Collision != null)
            {
                return extensions.Collision.CollisionTree.Bounds;
            }            

            // Try to use vertices of the mesh
            BoundingBox result = new BoundingBox();
#if !SILVERLIGHT
            if (ComputeBoundingBoxFromVertices(model, out result))
                return result;
#endif
            // Now use bounding spheres
            foreach (var mesh in model.Meshes)
            {
                BoundingBox box = BoundingBox.CreateFromSphere(
                    mesh.BoundingSphere.Transform(GetAbsoluteTransform(mesh, Matrix.Identity)));
                result = BoundingBox.CreateMerged(result, box);
            }

            return result;
        }  

#if !SILVERLIGHT
        static WeakReference<Vector3[]> WeakVertices = new WeakReference<Vector3[]>(null);
        static WeakReference<ushort[]> WeakIndices = new WeakReference<ushort[]>(null);
                  
        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        private static bool ComputeBoundingBoxFromVertices(this Microsoft.Xna.Framework.Graphics.Model model, out BoundingBox boundingBox)
        {
            if (null == model || model.Meshes.Count <= 0)
            {
                boundingBox = new BoundingBox();
                return false;
            }

            bool first = true;
            BoundingBox temp = new BoundingBox();

            Matrix[] bones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones);

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    if (part.VertexBuffer.BufferUsage == BufferUsage.WriteOnly)
                    {
                        boundingBox = new BoundingBox();
                        return false;
                    }        
                    if (!ComputeBoundingBoxFromVertices(model, mesh, part, bones[mesh.ParentBone.Index], out boundingBox))
                    {
                        boundingBox = new BoundingBox();
                        return false;
                    }
                    if (first)
                        temp = boundingBox;
                    else
                        BoundingBox.CreateMerged(ref temp, ref boundingBox, out temp);

                    first = false;
                }
            }

            boundingBox = temp;
            return true;
        }
#endif

        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        public static BoundingBox ComputeBoundingBox(this Microsoft.Xna.Framework.Graphics.Model model, Microsoft.Xna.Framework.Graphics.ModelMesh mesh, ModelMeshPart part)
        {
#if !SILVERLIGHT
            // Try to use vertices of the mesh
            BoundingBox result = new BoundingBox();
            if (ComputeBoundingBoxFromVertices(model, mesh, part, null, out result))
                return result;
#endif
            // Now use bounding spheres
            return BoundingBox.CreateFromSphere(mesh.BoundingSphere);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Copies the positions of the model to the target array.
        /// </summary>
        public static int CopyPositionsTo(this Microsoft.Xna.Framework.Graphics.Model model, Vector3[] positions, int startIndex)
        {
            Matrix[] bones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones);

            int count = 0;
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    int partCount = CopyPositionsTo(model, mesh, part, positions, startIndex);
                    if (positions != null)
                    {
                        for (int i = startIndex; i < startIndex + partCount; i++)
                        {
                            Vector3.Transform(ref positions[i], ref bones[mesh.ParentBone.Index], out positions[i]);
                        }
                    }
                    startIndex += partCount;
                    count += partCount;
                }
            }
            return count;
        }

        /// <summary>
        /// Copies the indices of the model to the target array.
        /// </summary>
        public static int CopyIndicesTo(this Microsoft.Xna.Framework.Graphics.Model model, ushort[] indices, int startIndex)
        {
            int count = 0;
            int baseVertex = 0;
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    int partCount = CopyIndicesTo(model, mesh, part, indices, startIndex);
                    if (indices != null)
                    {
                        for (int i = startIndex; i < startIndex + partCount; i++)
                        {
                            // TODO: May overflow here
                            indices[i] = (ushort)(baseVertex + indices[i]);
                        }
                    }
                    startIndex += partCount;
                    count += partCount;
                    baseVertex += part.NumVertices;
                }
            }
            return count;
        }

        /// <summary>
        /// Copies the positions of the model mesh part to the target array.
        /// The positions are not transformed by the transform of the model mesh.
        /// </summary>
        private static int CopyPositionsTo(this Microsoft.Xna.Framework.Graphics.Model model, Microsoft.Xna.Framework.Graphics.ModelMesh mesh, ModelMeshPart part, Vector3[] positions, int startIndex)
        {
            int stride = part.VertexBuffer.VertexDeclaration.VertexStride;
            int elementCount = part.NumVertices;

            if (positions != null)
            {
                part.VertexBuffer.GetData<Vector3>(part.VertexOffset * stride, positions, startIndex, elementCount, stride);
            }
            return elementCount;
        }

        /// <summary>
        /// Copies the indices of the model mesh part to the target array.
        /// </summary>
        private static int CopyIndicesTo(this Microsoft.Xna.Framework.Graphics.Model model, Microsoft.Xna.Framework.Graphics.ModelMesh mesh, ModelMeshPart part, ushort[] indices, int startIndex)
        {
            int indexCount = part.PrimitiveCount * 3;
            if (part.IndexBuffer.IndexElementSize == IndexElementSize.ThirtyTwoBits)
                throw new NotSupportedException();

            if (indices != null)
                part.IndexBuffer.GetData<ushort>(part.StartIndex * 2, indices, startIndex, indexCount);
            return indexCount;
        }

        /// <summary>
        /// Computes the bounding box for the specified xna model.
        /// </summary>
        private static bool ComputeBoundingBoxFromVertices(this Microsoft.Xna.Framework.Graphics.Model model, Microsoft.Xna.Framework.Graphics.ModelMesh mesh, ModelMeshPart part, Matrix? transform, out BoundingBox boundingBox)
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
            int elementCount = part.NumVertices;
            int indexCount = part.PrimitiveCount * 3;
            int indexBytes = part.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4;

            var Vertices = WeakVertices.Target;
            var Indices = WeakIndices.Target;
            if (Vertices == null || Vertices.Length < elementCount)
                WeakVertices.Target = Vertices = new Vector3[elementCount];
            if (Indices == null || Indices.Length < indexCount)
                WeakIndices.Target = Indices = new ushort[indexCount];
            
            part.VertexBuffer.GetData<Vector3>(part.VertexOffset * stride, Vertices, 0, elementCount, stride);
            part.IndexBuffer.GetData<ushort>(part.StartIndex * indexBytes, Indices, 0, indexCount);

            Matrix mx = new Matrix();
            if (transform != null)
                mx = transform.Value;

            for (int i = 0; i < indexCount; i++)
            {
                Vector3 v = Vertices[Indices[i]];
                if (transform != null)
                {
                    Vector3.Transform(ref v, ref mx, out v);
                }

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
#endif
    }
}
