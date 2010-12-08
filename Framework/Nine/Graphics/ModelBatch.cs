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
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines model sort-rendering options.
    /// </summary>
    public enum ModelSortMode
    {
        /// <summary>
        /// Models are not drawn until End is called. End will apply graphics device
        /// settings and draw all the sprites in one batch, in the same order calls to
        /// Draw were received. This mode allows Draw calls to two or more instances
        /// of ModelBatch without introducing conflicting graphics device settings.
        /// ModelBatch defaults to Deferred mode.
        /// </summary>
        Deferred = 0,

        /// <summary>
        /// Begin will apply new graphics device settings, and models will be drawn
        /// within each Draw call. In Immediate mode there can only be one active 
        /// ModelBatch instance without introducing conflicting device settings.
        /// </summary>
        Immediate = 1,

        /// <summary>
        /// Same as Deferred mode, except models are sorted by effect prior to drawing.
        /// This can improve performance when drawing opaque models of uniform depth.
        /// </summary>
        Effect = 2,

        /// <summary>
        /// Same as Deferred mode, except models are sorted by depth in back-to-front
        /// order prior to drawing. This procedure is recommended when drawing transparent
        /// models of varying depths.
        /// </summary>
        BackToFront = 3,

        /// <summary>
        /// Same as Deferred mode, except models are sorted by depth in front-to-back
        /// order prior to drawing. This procedure is recommended when drawing opaque
        /// models of varying depths.
        /// </summary>
        FrontToBack = 4,
    }

    internal class ModelBatchItem
    {
        public int Rank;
        public Effect Effect;
        public Texture2D Texture;
        public IndexBuffer IndexBuffer;
        public int VertexOffset;
        public int NumVertices;
        public int PrimitiveCount;
        public int StartIndex;
        public VertexBuffer VertexBuffer;
        public Matrix World;
        public Matrix[] BoneTransforms;
        public BoundingSphere BoundingSphere;
    }

    internal class ModelBatchSortComparer : IComparer<ModelBatchItem>
    {
        public int Compare(ModelBatchItem x, ModelBatchItem y)
        {
            return x.Rank - y.Rank;
        }
    }

    /// <summary>
    /// Enables a group of models to be drawn with different custom effects.
    /// </summary>
    public class ModelBatch
    {
        static SkinnedEffect basicSkinnedEffect;
        static BasicEffect basicEffect;

        private ModelSortMode sort;
        private bool hasBegin = false;
        private Model lastModel = null;
        private Matrix[] meshTransforms;
        private List<ModelBatchItem> batches;
        private int batchCount;
        private ModelBatchSortComparer comparer;
        private Vector3 eyePosition;

        /// <summary>
        /// Gets the view matrix used by this ModelBatch.
        /// </summary>
        public Matrix View { get; private set; }

        /// <summary>
        /// Gets the projection matrix used by this ModelBatch.
        /// </summary>
        public Matrix Projection { get; private set; }

        /// <summary>
        /// Gets the underlying graphics device used by this ModelBatch.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Creates a new ModelBatch instance.
        /// </summary>
        public ModelBatch(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
        }

        public void Begin(Matrix view, Matrix projection)
        {
            Begin(ModelSortMode.Deferred, view, projection);
        }

        public void Begin(ModelSortMode sortMode, Matrix view, Matrix projection)
        {
            View = view;
            Projection = projection;

            eyePosition = Matrix.Invert(view).Translation;

            sort = sortMode;
            hasBegin = true;

            lastModel = null;
            batchCount = 0;
        }

        public void Draw(Model model, Matrix world, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
                Draw(model, mesh, world, effect);
        }

        public void DrawSkinned(Model model, Matrix world, Matrix[] boneTransforms, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
                DrawSkinned(model, mesh, world, boneTransforms, effect);
        }

        public void Draw(Model model, ModelMesh mesh, Matrix world, Effect effect)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
                Draw(model, mesh, part, world, effect);
        }

        public void DrawSkinned(Model model, ModelMesh mesh, Matrix world, Matrix[] boneTransforms, Effect effect)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
                DrawSkinned(model, mesh, part, world, boneTransforms, effect);
        }

        public void Draw(Model model, ModelMesh mesh, ModelMeshPart part, Matrix world, Effect effect)
        {
            DrawSkinned(model, mesh, part, world, null, effect);
        }

        public void DrawSkinned(Model model, ModelMesh mesh, ModelMeshPart part, Matrix world, Matrix[] boneTransforms, Effect effect)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (meshTransforms == null || meshTransforms.Length < model.Bones.Count)
                meshTransforms = new Matrix[model.Bones.Count];

            // Hopefully we won't be copying bone transforms per model mesh part.
            if (model != lastModel)
            {
                lastModel = model;
                model.CopyAbsoluteBoneTransformsTo(meshTransforms);
            }

            // First try to use the model effect if none is specfied.
            if (effect == null)
                effect = part.Effect;

            effect = ApplyDefaultEffectWhenNull(boneTransforms, effect);

            Texture2D texture = part.Effect.GetTexture();

            DrawVerticesSkinned(part.VertexBuffer, part.IndexBuffer, part.VertexOffset, part.NumVertices, part.StartIndex, part.PrimitiveCount, meshTransforms[mesh.ParentBone.Index] * world, boneTransforms, effect, texture, mesh.BoundingSphere);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void DrawVertices(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int numVertices, int startIndex, int primitiveCount, Matrix world, Effect effect, Texture2D texture, BoundingSphere? boundingSphere)
        {
            DrawVerticesSkinned(vertexBuffer, indexBuffer, vertexOffset, numVertices, startIndex, primitiveCount, world, null, effect, texture, boundingSphere);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void DrawVerticesSkinned(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int numVertices, int startIndex, int primitiveCount, Matrix world, Matrix[] boneTransforms, Effect effect, Texture2D texture, BoundingSphere? boundingSphere)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called first.");

            if (boundingSphere == null && (sort == ModelSortMode.BackToFront || sort == ModelSortMode.FrontToBack))
                throw new ArgumentNullException("boundingSphere must not be null when you specify ModelSortMode.BackToFront or ModelSortMode.FrontToBack.");

            effect = ApplyDefaultEffectWhenNull(boneTransforms, effect);

            if (sort == ModelSortMode.Immediate)
            {
                InternalDraw(vertexBuffer, indexBuffer, vertexOffset, numVertices, startIndex, primitiveCount, world, boneTransforms, effect, texture);
                return;
            }

            if (batches == null)
                batches = new List<ModelBatchItem>(128);

            while (batches.Count <= batchCount)
                batches.Add(new ModelBatchItem());

            ModelBatchItem item = batches[batchCount];

            item.VertexBuffer = vertexBuffer;
            item.IndexBuffer = indexBuffer;
            item.VertexOffset = vertexOffset;
            item.NumVertices = numVertices;
            item.StartIndex = startIndex;
            item.PrimitiveCount = primitiveCount;
            item.Effect = effect;
            item.World = world;
            item.Texture = texture;
            item.BoneTransforms = boneTransforms;

            if (boundingSphere.HasValue)
                item.BoundingSphere = boundingSphere.Value;

            ComputeRank(item, sort);

            batchCount++;
        }

        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called first.");

            if (batches == null || batchCount <= 0)
                return;

            if (comparer == null)
                comparer = new ModelBatchSortComparer();

            if (sort != ModelSortMode.Immediate && sort != ModelSortMode.Deferred)
                batches.Sort(0, batchCount, comparer);

            for (int i = 0; i < batchCount; i++)
            {
                ModelBatchItem item = batches[i];
                InternalDraw(item.VertexBuffer, item.IndexBuffer, item.VertexOffset, item.NumVertices, item.StartIndex, item.PrimitiveCount, item.World, item.BoneTransforms, item.Effect, item.Texture);
            }
        }

        private void InternalDraw(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int numVertices, int startIndex, int primitiveCount, Matrix world, Matrix[] boneTransforms, Effect effect, Texture2D texture)
        {
            // Setup matrices
            if (effect is IEffectMatrices)
            {
                IEffectMatrices matrices = effect as IEffectMatrices;
                matrices.World = world;
                matrices.View = View;
                matrices.Projection = Projection;
            }

            // Setup bones
            if (boneTransforms != null && effect is IEffectSkinned)
            {
                IEffectSkinned skinned = effect as IEffectSkinned;
                skinned.SkinningEnabled = true;
                skinned.SetBoneTransforms(boneTransforms);
            }
            else if (boneTransforms == null && effect is IEffectSkinned)
            {
                IEffectSkinned skinned = effect as IEffectSkinned;
                skinned.SkinningEnabled = false;
            }
            else if (boneTransforms != null && effect is SkinnedEffect)
            {
                SkinnedEffect skinned = effect as SkinnedEffect;
                skinned.SetBoneTransforms(boneTransforms);
            }

            // Setup diffuse texture
            if (texture != null)
                effect.SetTexture(texture);

            // Draw geometry
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.SetVertexBuffer(vertexBuffer, vertexOffset);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices,
                                                                                       startIndex,
                                                                                       primitiveCount);
            }
        }

        private void ComputeRank(ModelBatchItem item, ModelSortMode sort)
        {
            float distance;
            float depth;

            switch (sort)
            {
                case ModelSortMode.BackToFront:
                    distance = Vector3.Subtract(eyePosition, item.BoundingSphere.Center).Length();
                    depth = distance - item.BoundingSphere.Radius;
                    item.Rank = (int)(0x0000FFFF * depth / distance);
                    break;
                case ModelSortMode.FrontToBack:
                    distance = Vector3.Subtract(eyePosition, item.BoundingSphere.Center).Length();
                    depth = distance - item.BoundingSphere.Radius;
                    item.Rank = -(int)(0x0000FFFF * depth / distance);
                    break;
                case ModelSortMode.Effect:
                    int rank = 0;
                    rank |= (item.Effect.GetType().GetHashCode() & 0x000000FF) << 24;
                    rank |= (item.Effect.GetHashCode() & 0x000000FF) << 18;
                    if (item.Texture != null)
                        rank |= (item.Texture.GetHashCode() & 0x000000FF) << 8;
                    rank |= (item.VertexBuffer.GetHashCode() & 0x000000FF);
                    item.Rank = rank;
                    break;
                case ModelSortMode.Deferred:
                case ModelSortMode.Immediate:
                default:
                    item.Rank = 0;
                    break;
            }
        }

        private Effect ApplyDefaultEffectWhenNull(Matrix[] boneTransforms, Effect effect)
        {
            // Setup default effect
            if (effect == null)
            {
                if (boneTransforms != null)
                {
                    if (basicSkinnedEffect == null)
                    {
                        basicSkinnedEffect = new SkinnedEffect(GraphicsDevice);
                        basicSkinnedEffect.WeightsPerVertex = 4;
                        basicSkinnedEffect.PreferPerPixelLighting = true;
                        basicSkinnedEffect.EnableDefaultLighting();
                    }

                    effect = basicSkinnedEffect;
                }
                else
                {
                    if (basicEffect == null)
                    {
                        basicEffect = new BasicEffect(GraphicsDevice);
                        basicEffect.TextureEnabled = true;
                        basicEffect.VertexColorEnabled = true;
                        basicEffect.PreferPerPixelLighting = true;
                        basicEffect.EnableDefaultLighting();
                    }

                    effect = basicEffect;
                }
            }

            return effect;
        }
    }
}
