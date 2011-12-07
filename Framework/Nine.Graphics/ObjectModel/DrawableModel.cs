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
#if WINDOWS || XBOX
using Nine.Graphics.Effects.Deferred;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines a basic model that can be rendered using the renderer with custom effect.
    /// </summary>
    public class DrawableModel : Transformable, ISpatialQueryable, IUpdateable, IPickable
    {
        #region Model
        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the underlying model.
        /// </summary>
        [ContentSerializer]
        public Microsoft.Xna.Framework.Graphics.Model Model
        {
            get { return model; }
            set
            {
                if (model != value)
                {
                    model = value;
                    if (model == null)
                        throw new ContentLoadException("Must specify a model.");
                    UpdateModel();
                }
            }
        }
        private Microsoft.Xna.Framework.Graphics.Model model;
        #endregion

        #region Material
        /// <summary>
        /// Gets or sets the material used to draw the model.
        /// </summary>
        /// <remarks>
        /// Setting the material of the model will override any existing materials of the <c>ModelPart</c>
        /// owned by this <c>Model</c> except for texture settings.
        /// To specify material for each individual model part, see <c>ModelPart.Material</c>.
        /// </remarks>
        public Material Material 
        {
            get 
            {
                UpdateMaterials();
                return material; 
            }
            set 
            {
                if (material != value || value == null)
                {
                    material = value;
                    materialNeedsUpdate = true;
                    UpdateMaterials(true);
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
        public bool OverrideModelMaterial
        {
            get { return overrideModelMaterial; }
            set
            {
                if (overrideModelMaterial != value)
                {
                    overrideModelMaterial = value;
                    materialNeedsUpdate = true;
                }
            }
        }
        bool overrideModelMaterial;

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
                var obj = material.Find<T>();
                if (obj != null)
                    action(obj);
            }
        }
        #endregion

        #region Lighting
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Model"/> is visible.
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
                if (value != null && modelParts != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (i < modelParts.Length && value[i] != null)
                        {
                            if (value[i].Material != null)
                            {
                                modelParts[i].Material = CreateMaterial(
                                    value[i].Material, modelParts[i].ModelMeshPart, false);
                            }
                            modelParts[i].Visible = value[i].Visible;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of child objects
        /// </summary>
        protected override int ChildCount
        {
            get { return modelParts.Length; }
        }

        /// <summary>
        /// Copies all the child objects to the target array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="startIndex"></param>
        public override void CopyTo(object[] array, int startIndex)
        {
            modelParts.CopyTo(array, startIndex);
        }
        #endregion

        #region BoundingBox
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public BoundingBox BoundingBox
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
            boundingBox = orientedBoundingBox.CreateAxisAligned(AbsoluteTransform);
            base.OnTransformChanged();
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;
        #endregion

        #region Animation
        /// <summary>
        /// Gets the animations.
        /// </summary>
        public AnimationPlayer Animations { get { return animations ?? (animations = new AnimationPlayer()); } }
        private AnimationPlayer animations;

        /// <summary>
        /// Gets the skeleton of this model.
        /// </summary>
        public ModelSkeleton Skeleton { get; private set; }

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
        #endregion

        #region Initialization
        internal DrawableModel() 
        {
            Visible = true;
            CastShadow = true;
            ReceiveShadow = false;
            LightingEnabled = true;
            MaxAffectingLights = 4;
            MaxReceivedShadows = 1;
            overrideModelMaterial = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableModel"/> class.
        /// </summary>
        public DrawableModel(Model model, Material material) : this()
        {
            if (model == null)
                throw new ArgumentNullException();
            this.Model = model;
            this.Material = material;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableModel"/> class.
        /// </summary>
        public DrawableModel(Microsoft.Xna.Framework.Graphics.Model model) : this(model, null) { }

        private void UpdateModel()
        {
            if (modelParts != null)
                throw new InvalidOperationException();

            if (model.Meshes.Count <= 0 || model.Meshes[0].MeshParts.Count <= 0 || model.Meshes[0].MeshParts[0].VertexBuffer == null)
                throw new ArgumentException("The input model is must have at least 1 valid mesh part.");
            
            // Initialize graphics device & material
#if SILVERLIGHT
            GraphicsDevice = System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice;
#else      
            GraphicsDevice = model.Meshes[0].MeshParts[0].VertexBuffer.GraphicsDevice;
#endif
            materialNeedsUpdate = true;

            // Initialize bounds
            orientedBoundingBox = model.ComputeBoundingBox();
            boundingBox = orientedBoundingBox.CreateAxisAligned(AbsoluteTransform);

            // Initialize animations
            Skeleton = new ModelSkeleton(model);

            var animationNames = model.GetAnimations();
            if (animationNames.Count > 0)
            {
                animations = new AnimationPlayer();
                animationNames.ForEach(anim => animations.Animations.Add(
                                   anim, new BoneAnimation(Skeleton, model.GetAnimation(anim))));
                animations.Play();
            }

            // Initialize parts
            var iPart = 0;
            modelParts = new DrawableModelPart[model.Meshes.Sum(m => m.MeshParts.Count)];
            ModelParts = new ReadOnlyCollection<DrawableModelPart>(modelParts);

            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    modelParts[iPart++] = new DrawableModelPart(this, mesh, part, null);
                }
            }
        }

        private void UpdateIsSkinnedAndIsAnimated()
        {
            IsSkinned = model.IsSkinned();
            if (material != null && IsSkinned)
            {
                var effectSkinned = material.Find<IEffectSkinned>();
                IsSkinned = effectSkinned != null && effectSkinned.SkinningEnabled;
            }

            // FIXME:
            IsAnimated = model.GetAnimations().Count > 0;
        }

        internal void UpdateMaterials()
        {
            UpdateMaterials(false);
        }

        private void UpdateMaterials(bool force)
        {
            if (model == null || !materialNeedsUpdate)
                return;

            // Set default materials based on whether the model is skinned or not
            if (material != null)
            {
                var effectMaterial = material.Find<IEffectMaterial>();
                if (effectMaterial != null)
                {
                    alpha = effectMaterial.Alpha;
                    diffuseColor = effectMaterial.DiffuseColor;
                }
            }

            // FIXME: What if only some part of the model is skinned
            UpdateIsSkinnedAndIsAnimated();

            bool isDefault = false;

#if !TEXT_TEMPLATE
            if (material == null)
            {
                isDefault = true;
                if (model.IsSkinned())
                    material = new SkinnedMaterial(GraphicsDevice);
                else
                    material = new BasicMaterial(GraphicsDevice);
            }
#endif
            var iPart = 0;
            foreach (var mesh in model.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    modelParts[iPart].Material = CreateMaterial(
                        force ? material :
                        modelParts[iPart].material ?? material, part, isDefault);
                    iPart++;
                }
            }
            materialNeedsUpdate = false;
        }

        private Material CreateMaterial(Material material, ModelMeshPart part, bool isDefault)
        {
            material.Apply();
            var clonedMaterial = material.Clone();

            if (!overrideModelTextures)
            {
                IEffectTexture target = clonedMaterial.Find<IEffectTexture>();
                if (target != null)
                {
                    target.Texture = part.Effect.GetTexture();
#if !TEXT_TEMPLATE
                    // Walkaround for default material when the model has no texture.
                    if (isDefault && target is BasicMaterial)
                    {
                        ((BasicMaterial)target).TextureEnabled = (target.Texture != null);
                    }
#endif
                    foreach (var texture in part.GetTextures())
                    {
                        target.SetTexture(texture, part.GetTexture(texture));
                    }
                }
            }

            if (!overrideModelMaterial)
            {
                IEffectMaterial source = part.Effect.As<IEffectMaterial>();
                IEffectMaterial target = clonedMaterial.Find<IEffectMaterial>();
                
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
                    if (skinTransforms == null || skinTransforms.Length < Skeleton.InverseAbsoluteBindPose.Count)
                    {
                        skinTransforms = new Matrix[Skeleton.InverseAbsoluteBindPose.Count];
                        Skeleton.GetSkinTransforms(skinTransforms);
                    }
                    else if (boneTransformNeedUpdate)
                    {
                        Skeleton.GetSkinTransforms(skinTransforms);
                    }
                }
                else
                {
                    if (boneTransforms == null || boneTransforms.Length < Model.Bones.Count)
                    {
                        boneTransforms = new Matrix[Model.Bones.Count];
                        Skeleton.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    }
                    else if (boneTransformNeedUpdate)
                    {
                        Skeleton.CopyAbsoluteBoneTransformsTo(boneTransforms);
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
    }
}