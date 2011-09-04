#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
        public IEffectInstance EffectInstance;
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
    public class ModelBatch : IDisposable
    {
        private ModelSortMode sort;
        private bool hasBegin = false;
        private Model lastModel = null;
        private Matrix[] meshTransforms;
        private List<ModelBatchItem> batches;
        private int batchCount;
        private ModelBatchSortComparer comparer;
        private Vector3 eyePosition;

        internal BlendState BlendState;
        internal SamplerState SamplerState;
        internal DepthStencilState DepthStencilState;
        internal RasterizerState RasterizerState;

        private Matrix view;
        private Matrix projection;

        private BasicEffect basicEffect;
        private SkinnedEffect skinnedEffect;

        internal int VertexCount { get; private set; }
        internal int PrimitiveCount { get; private set; }

        /// <summary>
        /// Gets the underlying graphics device used by this ModelBatch.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }


        /// <summary>
        /// Creates a new ModelBatch instance.
        /// </summary>
        public ModelBatch(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;

            basicEffect = (BasicEffect)GraphicsResources<BasicEffect>.GetInstance(GraphicsDevice).Clone();
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = true;
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();

            skinnedEffect = (SkinnedEffect)GraphicsResources<SkinnedEffect>.GetInstance(GraphicsDevice).Clone();
            skinnedEffect.WeightsPerVertex = 4;
            skinnedEffect.PreferPerPixelLighting = true;
            skinnedEffect.EnableDefaultLighting();
        }

        public void Begin(Matrix view, Matrix projection)
        {
            Begin(ModelSortMode.Deferred, view, projection);
        }

        public void Begin(ModelSortMode sortMode, Matrix view, Matrix projection)
        {
            Begin(sort, view, projection, null , null, null, null);
        }
        
        public void Begin(ModelSortMode sortMode, Matrix view, Matrix projection, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
        {
            if (hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            this.VertexCount = 0;
            this.PrimitiveCount = 0;
            this.BlendState = blendState != null ? blendState : BlendState.Opaque;
            this.SamplerState = samplerState != null ? samplerState : SamplerState.LinearWrap;
            this.DepthStencilState = depthStencilState != null ? depthStencilState : DepthStencilState.Default;
            this.RasterizerState = rasterizerState != null ? rasterizerState : RasterizerState.CullCounterClockwise;

            this.view = view;
            this.projection = projection;

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

        public void Draw(Model model, ModelMesh mesh, ModelMeshPart part, Matrix world, Effect effect, IEffectInstance effectInstance)
        {
            DrawSkinned(model, mesh, part, world, null, effect, effectInstance);
        }

        public void DrawSkinned(Model model, ModelMesh mesh, ModelMeshPart part, Matrix world, Matrix[] boneTransforms, Effect effect)
        {
            DrawSkinned(model, mesh, part, world, boneTransforms, effect, null);
        }

        public void DrawSkinned(Model model, ModelMesh mesh, ModelMeshPart part, Matrix world, Matrix[] boneTransforms, Effect effect, IEffectInstance effectInstance)
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

            if (effect == null && effectInstance != null)
                effect = effectInstance.Effect;

            // First try to use the model effect if none is specfied.
            if (effect == null)
                effect = part.Effect;

            effect = ApplyDefaultEffectWhenNull(boneTransforms, effect);

            Texture2D texture = part.Effect.GetTexture();

            DrawVerticesSkinned(part.VertexBuffer, part.IndexBuffer, part.VertexOffset, part.NumVertices, part.StartIndex, part.PrimitiveCount, meshTransforms[mesh.ParentBone.Index] * world, boneTransforms, effect, effectInstance, texture, mesh.BoundingSphere);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void DrawVertices(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int numVertices, int startIndex, int primitiveCount, Matrix world, Effect effect, IEffectInstance effectInstance, Texture2D texture, BoundingSphere? boundingSphere)
        {
            DrawVerticesSkinned(vertexBuffer, indexBuffer, vertexOffset, numVertices, startIndex, primitiveCount, world, null, effect, effectInstance, texture, boundingSphere);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void DrawVerticesSkinned(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int numVertices, int startIndex, int primitiveCount, Matrix world, Matrix[] boneTransforms, Effect effect, IEffectInstance effectInstance, Texture2D texture, BoundingSphere? boundingSphere)
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            if (boundingSphere == null && (sort == ModelSortMode.BackToFront || sort == ModelSortMode.FrontToBack))
                throw new ArgumentNullException("boundingSphere must not be null when you specify ModelSortMode.BackToFront or ModelSortMode.FrontToBack.");

            if (effect == null)
                effect = effectInstance.Effect;

            effect = ApplyDefaultEffectWhenNull(boneTransforms, effect);

            if (sort == ModelSortMode.Immediate)
            {
                InternalDraw(vertexBuffer, indexBuffer, vertexOffset, numVertices, startIndex, primitiveCount, world, boneTransforms, effect, effectInstance, texture);
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
            item.EffectInstance = effectInstance;
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
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            hasBegin = false;

            if (batches == null || batchCount <= 0)
                return;

            if (comparer == null)
                comparer = new ModelBatchSortComparer();

            if (sort != ModelSortMode.Immediate && sort != ModelSortMode.Deferred)
                batches.Sort(0, batchCount, comparer);

            for (int i = 0; i < batchCount; i++)
            {
                ModelBatchItem item = batches[i];
                InternalDraw(item.VertexBuffer, item.IndexBuffer, item.VertexOffset, item.NumVertices, item.StartIndex, item.PrimitiveCount, item.World, item.BoneTransforms, item.Effect, item.EffectInstance, item.Texture);
            }
        }

        private void InternalDraw(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, int vertexOffset, int numVertices, int startIndex, int primitiveCount, Matrix world, Matrix[] boneTransforms, Effect effect, IEffectInstance effectInstance, Texture2D texture)
        {
            // Setup matrices
            if (effect is IEffectMatrices)
            {
                IEffectMatrices matrices = effect as IEffectMatrices;
                matrices.World = world;
                matrices.View = view;
                matrices.Projection = projection;
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

            // Setup state
            GraphicsDevice.BlendState = BlendState;
            GraphicsDevice.DepthStencilState = DepthStencilState;
            GraphicsDevice.SamplerStates[0] = SamplerState;
            GraphicsDevice.RasterizerState = RasterizerState;

            if (effectInstance != null)
                effectInstance.Apply();

            // Draw geometry
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                VertexCount += numVertices;
                PrimitiveCount += primitiveCount;

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
                    effect = skinnedEffect;
                else
                    effect = basicEffect;
            }
            return effect;
        }
        
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (basicEffect != null)
                    basicEffect.Dispose();
                if (skinnedEffect != null)
                    skinnedEffect.Dispose();
            }
        }

        ~ModelBatch()
        {
            Dispose(false);
        }
    }
}
