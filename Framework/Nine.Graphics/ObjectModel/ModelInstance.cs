#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Animations;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    #region ModelInstance
    /// <summary>
    /// Defines a basic view of model with custom effect.
    /// </summary>
    public class ModelInstance : Drawable, IMaterial, ILightable, IEnumerable<Drawable>
    {
        #region Model
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Model Model
        {
            get { return model; }
            internal set
            {
                if (model != value)
                {
                    model = value;
                    if (model != null)
                    {
                        InitModel();
                    }
                }
            }
        }
        private Model model;
        #endregion

        #region Effect
        /// <summary>
        /// Gets or sets the effect used to draw the model.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public IEffectInstance Effect 
        {
            get { return effect; }
            set 
            {
                if (effect != value)
                {
                    effect = value;
                    if (effect != null)
                    {
                        effect = value;
                        InitEffects();
                    }
                }
            }
        }
        private IEffectInstance effect;
        #endregion

        #region BoundingBox
        public override BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }
        private BoundingBox orientedBoundingBox;
        private BoundingBox boundingBox;

        protected override void OnTransformChanged()
        {
            boundingBox = orientedBoundingBox.CreateAxisAligned(Transform);
            base.OnTransformChanged();
        }
        #endregion

        #region Animation
        public AnimationPlayer Animations { get { return animations ?? (animations = new AnimationPlayer()); } }
        private AnimationPlayer animations;
        private ModelSkeleton skeleton;

        public bool IsSkinned { get; private set; }
        public bool IsAnimated { get; private set; }
        #endregion

        #region Material
        public bool IsTransparent { get { /* TODO: */ return false; } }
        public bool CastShadow { get; set; }
        public bool ReceiveShadow { get; set; }
        #endregion

        #region Initialization
        internal ModelInstance() { }

        public ModelInstance(Model model, IEffectInstance effect)
        {
            if (model == null)
                throw new ArgumentNullException();
            this.Model = model;
            this.Effect = effect;
        }

        private static void ForEachModelMeshPart(Model model, Action<ModelMesh, ModelMeshPart> action)
        {
            if (action != null)
                foreach (var mesh in model.Meshes)  
                    foreach (var part in mesh.MeshParts)
                        action(mesh, part);
        }

        private void InitModel()
        {
            orientedBoundingBox = model.ComputeBoundingBox();
            boundingBox = orientedBoundingBox.CreateAxisAligned(Transform);

            var animationNames = model.GetAnimations();
            if (animationNames.Count > 0)
            {
                skeleton = new ModelSkeleton(model);
                animations = new AnimationPlayer();
                animationNames.ForEach(anim => animations.Animations.Add(
                                   anim, new BoneAnimation(skeleton, model.GetAnimation(anim))));
                animations.Play();
            }

            var iPart = 0;
            partInstances = new ModelMeshPartInstance[model.Meshes.Sum(m => m.MeshParts.Count)];
            ForEachModelMeshPart(model, (mesh, part) => partInstances[iPart++] = 
                        new ModelMeshPartInstance(this, mesh, part, CreateEffectInstance(effect, part)));

            IsSkinned = model.IsSkinned();
            IsAnimated = model.GetAnimations().Count > 0;
        }

        private void InitEffects()
        {
            if (model == null)
                return;
            var iPart = 0;
            ForEachModelMeshPart(model, (mesh, part) => partInstances[iPart++].Effect = CreateEffectInstance(effect, part));
        }

        private IEffectInstance CreateEffectInstance(IEffectInstance effectInstance, ModelMeshPart part)
        {
            var effect = part.Effect;
            if (effectInstance == null)
            {
#if !TEXT_TEMPLATE
                if (effect is BasicEffect)
                    return new BasicEffectInstance(effect as BasicEffect);
                if (effect is SkinnedEffect)
                    return new SkinnedEffectInstance(effect as SkinnedEffect);
                if (effect is EnvironmentMapEffect)
                    return new EnvironmentMapEffectInstance(effect as EnvironmentMapEffect);
                if (effect is DualTextureEffect)
                    return new DualTextureEffectInstance(effect as DualTextureEffect);
                if (effect is AlphaTestEffect)
                    return new AlphaTestEffectInstance(effect as AlphaTestEffect);
#endif
                return new EffectInstance(effect);
            }

            // Hack
            effectInstance.Apply();
            var tempEffect = part.Effect;
            part.ConvertEffectTo(effectInstance.Effect);
            part.Effect = tempEffect;
            return (IEffectInstance)Activator.CreateInstance(effectInstance.GetType(), effectInstance.Effect);
        }
        #endregion

        #region BoneTransform
        bool boneTransformNeedUpdate = false;

        private void UpdateBoneTransforms()
        {
            if (boneTransformNeedUpdate)
            {
                if (IsAnimated)
                {
                    if (IsSkinned)
                    {
                        if (skinTransforms == null || skinTransforms.Length < skeleton.InverseAbsoluteBindPose.Count)
                            skinTransforms = new Matrix[skeleton.InverseAbsoluteBindPose.Count];
                        skeleton.GetSkinTransforms(skinTransforms);
                    }
                    else
                    {
                        if (boneTransforms == null || boneTransforms.Length < Model.Bones.Count)
                            boneTransforms = new Matrix[Model.Bones.Count];
                        skeleton.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    }
                }
                boneTransformNeedUpdate = false;
            }
        }

        Matrix[] skinTransforms;
        static Matrix[] boneTransforms;
        #endregion

        #region Draw
        public override void Update(TimeSpan elapsedTime)
        {
            if (animations != null)
            {
                animations.Update(elapsedTime);
                boneTransformNeedUpdate = true;
            }
        }

        // TODO: Delegate this to ModelMeshPartInstance ??
        public override void Draw(GraphicsContext context) { }
        
        internal void DrawPart(GraphicsContext context, ModelMesh mesh, ModelMeshPart part, IEffectInstance effect)
        {
            UpdateBoneTransforms();

            if (IsAnimated)
                if (IsSkinned)
                    context.ModelBatch.DrawSkinned(Model, mesh, part, AbsoluteTransform, skinTransforms, null, effect);
                else
                    context.ModelBatch.Draw(Model, mesh, part, boneTransforms[mesh.ParentBone.Index] * AbsoluteTransform, null, effect);
            else
                context.ModelBatch.Draw(Model, mesh, part, AbsoluteTransform, null, effect);
        }

        internal void DrawPart(GraphicsContext context, ModelMesh mesh, ModelMeshPart part, Effect effect)
        {
            UpdateBoneTransforms();

            if (IsAnimated)
                if (IsSkinned)
                    context.ModelBatch.DrawSkinned(Model, mesh, part, AbsoluteTransform, skinTransforms, effect);
                else
                    context.ModelBatch.Draw(Model, mesh, part, boneTransforms[mesh.ParentBone.Index] * AbsoluteTransform, effect);
            else
                context.ModelBatch.Draw(Model, mesh, part, AbsoluteTransform, effect);
        }
        #endregion
        
        #region IEnumerable
        private ModelMeshPartInstance[] partInstances;
        public IEnumerator<Drawable> GetEnumerator()
        {
            return partInstances.OfType<Drawable>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    #endregion

    #region ModelMeshPartInstance
    [NotContentSerializable]
    class ModelMeshPartInstance : Drawable, IMaterial, ILightable
    {
        private ModelInstance model;
        private ModelMesh mesh;
        private ModelMeshPart part;

        #region BoundingBox
        public override BoundingBox BoundingBox
        {
            // TODO:
            // BoundingBox works on static and rigid animated models, 
            // but doesn't work for skinned models.
            get { return model.BoundingBox; }
        }
        private BoundingBox orientedBoundingBox;
        private BoundingBox boundingBox;

        protected override void OnTransformChanged()
        {
            boundingBox = orientedBoundingBox.CreateAxisAligned(Transform);
            base.OnTransformChanged();
        }
        #endregion

        public ModelMeshPartInstance(ModelInstance model, ModelMesh mesh, ModelMeshPart part, IEffectInstance effectInstance)
        {
            if (model == null || part == null)
                throw new ArgumentNullException();

            this.model = model;
            this.mesh = mesh;
            this.part = part;
            this.Effect = effectInstance;
            this.orientedBoundingBox = model.Model.ComputeBoundingBox(mesh, part);
            this.Transform = mesh.GetAbsoluteTransform();
        }

        public IEffectInstance Effect { get; internal set; }
        public bool IsTransparent { get { return model.IsTransparent; } }
        public bool CastShadow { get { return model.CastShadow; } }
        public bool ReceiveShadow { get { return model.ReceiveShadow; } }

        public override void Draw(GraphicsContext context) 
        {
            model.DrawPart(context, mesh, part, Effect);
        }

        public override void Draw(GraphicsContext context, Effect effect)
        {
            model.DrawPart(context, mesh, part, effect);
        }
    }
    #endregion
}