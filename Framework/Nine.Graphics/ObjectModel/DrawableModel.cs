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
using Nine.Graphics.Effects;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects.Deferred;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines a basic model that can be rendered using the renderer with custom effect.
    /// </summary>
    public class DrawableModel : Transformable, IUpdateable, IPickable, IEnumerable<DrawableModelPart>
    {
        #region Model
        /// <summary>
        /// Gets or sets the underlying model.
        /// </summary>
        [ContentSerializer]
        public Model Model
        {
            get { return model; }
            internal set
            {
                if (model != value)
                {
                    model = value;
                    if (model == null)
                        throw new ContentLoadException("DrawableModel must specify a model.");
                    UpdateModel();
                }
            }
        }
        private Model model;
        #endregion

        #region Material
        /// <summary>
        /// Gets or sets the material used to draw the model.
        /// </summary>
        /// <remarks>
        /// Setting the material of the model will override any existing materials of the <c>DrawableModelPart</c>
        /// owned by this <c>DrawableModel</c> except for texture settings.
        /// To specify material for each individual model part, see <c>DrawableModelPart.Material</c>.
        /// </remarks>
        [ContentSerializer]
        public Material Material 
        {
            get 
            {
                UpdateMaterials();
                return material; 
            }
            set 
            {
                if (material != value)
                {
                    material = value;
                    materialNeedsUpdate = true;
                }
            }
        }
        Material material;
        bool materialNeedsUpdate = false;

        /// <summary>
        /// Gets or sets whether the specified material will override
        /// the default model diffuse texture, normal map, diffuse map, etc.
        /// The default behavior is not override.
        /// </summary>
        public bool OverrideModelTextures
        {
            get { return overrideModelTextures; }
            set
            {
                if (overrideModelTextures != value)
                {
                    overrideModelTextures = value;
                    materialNeedsUpdate = true;
                }
            }
        }
        bool overrideModelTextures;

        /// <summary>
        /// Gets or sets whether the specified material will override
        /// the default model alpha, diffuse, specular, emissive color and specular
        /// power settings.
        /// The default behavior is to override.
        /// </summary>
        public bool OverrideModelColors
        {
            get { return overrideModelColors; }
            set
            {
                if (overrideModelColors != value)
                {
                    overrideModelColors = value;
                    materialNeedsUpdate = true;
                }
            }
        }
        bool overrideModelColors;

        /// <summary>
        /// Gets or sets the alpha of this model.
        /// </summary>
        public float Alpha
        {
            get
            {
                UpdateMaterials();
                return alpha; 
            }
            set 
            {
                if (alpha != value)
                {
                    UpdateMaterials();
                    alpha = value;
                    ForEachMaterial<IEffectMaterial>(mat => mat.Alpha = value);
                }
            }
        }
        private float alpha = 1;

        /// <summary>
        /// Gets or sets the diffuse color of this model.
        /// </summary>
        public Vector3 DiffuseColor
        {
            get
            {
                UpdateMaterials(); 
                return diffuseColor;
            }
            set
            {
                if (diffuseColor != value)
                {
                    UpdateMaterials();
                    diffuseColor = value;
                    ForEachMaterial<IEffectMaterial>(mat => mat.DiffuseColor = value);
                }
            }
        }
        private Vector3 diffuseColor = Vector3.One;

        private void ForEachMaterial<T>(Action<T> action) where T : class
        {
            for (int i = 0; i < modelParts.Length; i++)
            {
                var material = modelParts[i].Material;
                if (material == null)
                    continue;
                var obj = material.As<T>();
                if (obj != null)
                    action(obj);
            }
        }
        #endregion

        #region Lighting
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DrawableModel"/> is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lighting is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if lighting is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool LightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multi-pass lighting is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if multi-pass lighting is enabled; otherwise, <c>false</c>.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MultiPassLightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the max affecting lights.
        /// </summary>
        /// <value>
        /// The max affecting lights.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int MaxAffectingLights { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model casts shadow.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this model casts shadow; otherwise, <c>false</c>.
        /// </value>
        public bool CastShadow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model casts shadow.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this model casts shadow; otherwise, <c>false</c>.
        /// </value>
        public bool ReceiveShadow { get; set; }

        /// <summary>
        /// Gets or sets the max received shadows.
        /// </summary>
        /// <value>
        /// The max received shadows.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int MaxReceivedShadows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multi-pass shadowing is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if multi-pass shadowing is enabled; otherwise, <c>false</c>.
        /// </value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MultiPassShadowEnabled { get; set; }
        #endregion

        #region ModelParts
        /// <summary>
        /// Gets the model parts that made up of this model.
        /// </summary>
        public ReadOnlyCollection<DrawableModelPart> ModelParts { get; internal set; }
        private DrawableModelPart[] modelParts;

        [ContentSerializer(ElementName="ModelParts")]
        internal DrawableModelPart[] modelPartsSerializer 
        {
            get { return modelParts; }
            set 
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (i < modelParts.Length && value[i] != null)
                        {
                            if (value[i].Material != null)
                                modelParts[i].Material = CreateMaterial(value[i].Material, modelParts[i].ModelMeshPart);
                            modelParts[i].Visible = value[i].Visible;
                        }
                    }
                }
            }
        }
        #endregion

        #region BoundingBox
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public override BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }
        private BoundingBox orientedBoundingBox;
        private BoundingBox boundingBox;

        /// <summary>
        /// Called when transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            boundingBox = orientedBoundingBox.CreateAxisAligned(Transform);
            base.OnTransformChanged();
        }
        #endregion

        #region Animation
        /// <summary>
        /// Gets the animations.
        /// </summary>
        public AnimationPlayer Animations { get { return animations ?? (animations = new AnimationPlayer()); } }
        private AnimationPlayer animations;
        private ModelSkeleton skeleton;

        /// <summary>
        /// Gets a value indicating whether this instance is skinned.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is skinned; otherwise, <c>false</c>.
        /// </value>
        public bool IsSkinned { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is animated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is animated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnimated { get; private set; }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region Initialization
        internal DrawableModel() 
        {
            Visible = true;
            CastShadow = true;
            ReceiveShadow = true;
            LightingEnabled = true;
            MaxAffectingLights = 4;
            MaxReceivedShadows = 1;
            overrideModelColors = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableModel"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="material">The material.</param>
        public DrawableModel(Model model, Material material) : this()
        {
            if (model == null)
                throw new ArgumentNullException();
            this.Model = model;
            this.Material = material;
        }

        private static void ForEachModelMeshPart(Model model, Action<ModelMesh, ModelMeshPart> action)
        {
            if (action != null)
                foreach (var mesh in model.Meshes)  
                    foreach (var part in mesh.MeshParts)
                        action(mesh, part);
        }

        private void UpdateModel()
        {
            if (modelParts != null)
                throw new InvalidOperationException();

            // Initialize bounds
            materialNeedsUpdate = true;
            orientedBoundingBox = model.ComputeBoundingBox();
            boundingBox = orientedBoundingBox.CreateAxisAligned(Transform);

            // Initialize animations
            var animationNames = model.GetAnimations();
            if (animationNames.Count > 0)
            {
                skeleton = new ModelSkeleton(model);
                animations = new AnimationPlayer();
                animationNames.ForEach(anim => animations.Animations.Add(
                                   anim, new BoneAnimation(skeleton, model.GetAnimation(anim))));
                animations.Play();
            }

            // Initialize parts
            var iPart = 0;
            modelParts = new DrawableModelPart[model.Meshes.Sum(m => m.MeshParts.Count)];
            ModelParts = new ReadOnlyCollection<DrawableModelPart>(modelParts);

            ForEachModelMeshPart(model, (mesh, part) => modelParts[iPart++] = new DrawableModelPart(this, mesh, part, null));
        }

        private void UpdateIsSkinnedAndIsAnimated()
        {
            IsSkinned = model.IsSkinned();
            if (material != null && IsSkinned)
            {
                var effectSkinned = material.As<IEffectSkinned>();
                IsSkinned = effectSkinned != null && effectSkinned.SkinningEnabled;
            }
            IsAnimated = model.GetAnimations().Count > 0;
        }

        internal void UpdateMaterials()
        {
            if (model == null || material == null || !materialNeedsUpdate)
                return;

            if (material != null)
            {
                var effectMaterial = material.As<IEffectMaterial>();
                if (effectMaterial != null)
                {
                    alpha = effectMaterial.Alpha;
                    diffuseColor = effectMaterial.DiffuseColor;
                }
            }

            UpdateIsSkinnedAndIsAnimated();

            var iPart = 0;
            ForEachModelMeshPart(model, (mesh, part) => modelParts[iPart++].Material = CreateMaterial(material, part));

            materialNeedsUpdate = false;
        }

        private Material CreateMaterial(Material material, ModelMeshPart part)
        {
            if (material == null)
                return null;

            material.Apply();
            var clonedMaterial = material.Clone();

            if (!overrideModelTextures)
            {
                IEffectTexture target = clonedMaterial.As<IEffectTexture>();
                if (target != null)
                {
                    target.Texture = part.Effect.GetTexture();
                    foreach (var texture in part.GetTextures())
                    {
                        target.SetTexture(texture, part.GetTexture(texture));
                    }
                }
            }

            if (!overrideModelColors)
            {
                IEffectMaterial source = part.Effect.As<IEffectMaterial>();
                IEffectMaterial target = clonedMaterial.As<IEffectMaterial>();
                
                if (source != null && target != null)
                {
                    target.Alpha = source.Alpha;
                    target.DiffuseColor = source.DiffuseColor;
                    target.EmissiveColor = source.EmissiveColor;
                    target.SpecularColor = source.SpecularColor;
                    target.SpecularPower = source.SpecularPower;
                }
            }
            return clonedMaterial;
        }
        #endregion

        #region BoneTransform
        bool boneTransformNeedUpdate = false;

        private void UpdateBoneTransforms()
        {
            if (IsAnimated)
            {
                if (IsSkinned)
                {
                    if (skinTransforms == null || skinTransforms.Length < skeleton.InverseAbsoluteBindPose.Count)
                    {
                        skinTransforms = new Matrix[skeleton.InverseAbsoluteBindPose.Count];
                        skeleton.GetSkinTransforms(skinTransforms);
                    }
                    else if (boneTransformNeedUpdate)
                    {
                        skeleton.GetSkinTransforms(skinTransforms);
                    }
                }
                else
                {
                    if (boneTransforms == null || boneTransforms.Length < Model.Bones.Count)
                    {
                        boneTransforms = new Matrix[Model.Bones.Count];
                        skeleton.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    }
                    else if (boneTransformNeedUpdate)
                    {
                        skeleton.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    }
                }
            }
            boneTransformNeedUpdate = false;
        }

        Matrix[] skinTransforms;
        static Matrix[] boneTransforms;
        #endregion

        #region Draw
        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public void Update(TimeSpan elapsedTime)
        {
            if (animations != null)
            {
                animations.Update(elapsedTime);
                boneTransformNeedUpdate = true;
            }
        }

        internal void DrawPart(GraphicsContext context, DrawableModelPart drawableModelPart)
        {
            UpdateMaterials();
            UpdateBoneTransforms();

            var mesh = drawableModelPart.ModelMesh;
            var part = drawableModelPart.ModelMeshPart;

            if (IsAnimated)
                if (IsSkinned)
                    context.ModelBatch.DrawSkinned(Model, mesh, part, AbsoluteTransform, skinTransforms, null, drawableModelPart.Material);
                else
                    context.ModelBatch.Draw(Model, mesh, part, boneTransforms[mesh.ParentBone.Index] * AbsoluteTransform, null, drawableModelPart.Material);
            else
                context.ModelBatch.Draw(Model, mesh, part, AbsoluteTransform, null, drawableModelPart.Material);
        }

        internal void DrawPart(GraphicsContext context, DrawableModelPart drawableModelPart, Effect effect)
        {
            UpdateMaterials();
            UpdateBoneTransforms();

            var mesh = drawableModelPart.ModelMesh;
            var part = drawableModelPart.ModelMeshPart;

            if (IsAnimated)
                if (IsSkinned)
                    context.ModelBatch.DrawSkinned(Model, mesh, part, AbsoluteTransform, skinTransforms, effect);
                else
                    context.ModelBatch.Draw(Model, mesh, part, boneTransforms[mesh.ParentBone.Index] * AbsoluteTransform, effect);
            else
                context.ModelBatch.Draw(Model, mesh, part, AbsoluteTransform, effect);
        }
        #endregion

        #region IPickable
        /// <summary>
        /// Gets wether the object contains the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Vector3 point)
        {
            return model.Contains(AbsoluteTransform, point);
        }

        /// <summary>
        /// Gets the nearest intersection point from the specifed picking ray.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns>
        /// Distance to the start of the ray.
        /// </returns>
        public float? Intersects(Ray ray)
        {
            return model.Intersects(AbsoluteTransform, ray);
        }
        #endregion
        
        #region IEnumerable
        IEnumerator<DrawableModelPart> IEnumerable<DrawableModelPart>.GetEnumerator()
        {
            return modelParts.OfType<DrawableModelPart>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return modelParts.OfType<DrawableModelPart>().GetEnumerator();
        }
        #endregion
    }
}