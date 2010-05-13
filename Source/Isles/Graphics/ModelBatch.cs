#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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
using Isles.Graphics.Effects;
#endregion

namespace Isles.Graphics
{
    public enum ModelSortMode
    {
        Deferred = 0,
        Immediate = 1,
    }

    public class ModelBatch : IEffectFog
    {
        #region Batch Key & Value
        struct Key
        {
            public Effect Effect;
            public Texture Texture;
        }

        struct Value
        {
            public ModelMesh Mesh;
            public ModelMeshPart Part;
            public Matrix[] Bones;
            public Matrix World;
        }
        #endregion


        static SkinnedEffect basicSkinnedEffect;
        static BasicEffect basicEffect;


        private ModelSortMode sort;
        private bool hasBegin = false;
        private Model lastModel = null;
        private Matrix[] bones;

        // TODO: Use dynamic capacity rather then fixed capacity
        private Batch<Key, Value> batches;
        

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }


        public Vector3 FogColor { get; set; }
        public bool FogEnabled { get; set; }
        public float FogEnd { get; set; }
        public float FogStart { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }


        public ModelBatch(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;

            batches = new Batch<Key, Value>(32);
        }

        public ModelBatch(GraphicsDevice graphics, int capacity)
        {
            GraphicsDevice = graphics;

            batches = new Batch<Key, Value>(capacity);
        }

        public void Begin(Matrix view, Matrix projection)
        {
            Begin(ModelSortMode.Deferred, view, projection);
        }

        public void Begin(ModelSortMode sortMode, Matrix view, Matrix projection) 
        {
            View = view;
            Projection = projection;

            sort = sortMode;
            batches.Clear();
            hasBegin = true;

            lastModel = null;
        }

        public void Draw(Model model, Matrix world, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
                Draw(model, mesh, world, effect);
        }
        
        public void Draw(Model model, Matrix world, Matrix[] boneTransforms, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
                Draw(model, mesh, world, boneTransforms, effect);
        }

        public void Draw(Model model, ModelMesh mesh, Matrix world, Effect effect)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
                Draw(model, mesh, part, world, effect);
        }

        public void Draw(Model model, ModelMesh mesh, Matrix world, Matrix[] boneTransforms, Effect effect)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
                Draw(model, mesh, part, world, boneTransforms, effect);
        }

        public void Draw(Model model, ModelMesh mesh, ModelMeshPart part, Matrix world, Effect effect)
        {
            Draw(model, mesh, part, world, null, effect);
        }

        public void Draw(Model model, ModelMesh mesh, ModelMeshPart part, Matrix world, Matrix[] boneTransforms, Effect effect)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called first.");

            if (model == null)
                throw new ArgumentNullException("model");

            if (bones == null || bones.Length < model.Bones.Count)
                bones = new Matrix[model.Bones.Count];

            // Hopefully we won't be copying bone transforms per model mesh part.
            if (model != lastModel)
            {
                lastModel = model;
                model.CopyAbsoluteBoneTransformsTo(bones);
            }

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


            Key key;

            key.Effect = effect;
            key.Texture = GetPrimitiveTexture(part);


            Value value;

            value.Mesh = mesh;
            value.Part = part;
            value.Bones = boneTransforms;
            value.World = bones[mesh.ParentBone.Index] * world;


            if (sort == ModelSortMode.Deferred)
                batches.Add(key, value);
            else if (sort == ModelSortMode.Immediate)
                Draw(part, effect, value.World, boneTransforms);
        }

        private Texture GetPrimitiveTexture(ModelMeshPart part)
        {
            BasicEffect basic = part.Effect as BasicEffect;

            return basic != null ? basic.Texture : null;
        }

        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called first.");

            
            foreach (BatchItem<Key, Value> batch in batches.Batches)
            {
                for (int i = 0; i < batch.Count; i++)
                {
                    Value value = batch.Values[i];

                    Draw(value.Part, batch.Key.Effect, value.World, value.Bones);
                }
            }
        }

        private void Draw(ModelMeshPart part, Effect effect, Matrix world, Matrix[] boneTransforms)
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

            // Setup fog
            if (effect is IEffectFog)
            {
                IEffectFog fog = effect as IEffectFog;

                fog.FogColor = FogColor;
                fog.FogEnd = FogEnd;
                fog.FogStart = FogStart;
                fog.FogEnabled = FogEnabled;
            }

            // Setup material & texture
            if (part.Effect is BasicEffect)
            {
                BasicEffect basic = part.Effect as BasicEffect;

                if (effect is IEffectMaterial)
                {
                    IEffectMaterial material = effect as IEffectMaterial;

                    material.AmbientLightColor = basic.AmbientLightColor;
                    material.DiffuseColor = basic.DiffuseColor;
                    material.EmissiveColor = basic.EmissiveColor;
                    material.SpecularColor = basic.SpecularColor;
                    material.SpecularPower = basic.SpecularPower;
                    material.Texture = basic.Texture;
                }
                else if (effect is BasicEffect)
                {
                    (effect as BasicEffect).Texture = basic.Texture;
                }
                else if (effect is SkinnedEffect)
                {
                    (effect as SkinnedEffect).Texture = basic.Texture;
                }
                else if (effect is AlphaTestEffect)
                {
                    (effect as AlphaTestEffect).Texture = basic.Texture;
                }
                else if (effect is DualTextureEffect)
                {
                    (effect as DualTextureEffect).Texture = basic.Texture;
                }
            }

            // Draw geometry
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.Indices = part.IndexBuffer;
                GraphicsDevice.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices,
                                                                                       part.StartIndex,
                                                                                       part.PrimitiveCount);
            }
        }
    }
}
