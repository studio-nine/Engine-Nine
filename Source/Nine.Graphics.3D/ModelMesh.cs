namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Serialization;

    /// <summary>
    /// Defines a part of a model that contains only one material.
    /// </summary>
    [BinarySerializable]
    public class ModelMesh : Nine.Object, IDrawableObject, ILightable, Nine.IComponent
    {
        #region Properties
        /// <summary>
        /// Gets the containing model.
        /// </summary>
        public Model Model
        {
            get { return model; }
        }
        private Model model;

        /// <summary>
        /// Gets or sets the visibility of this model part.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        internal bool visible = true;

        /// <summary>
        /// Gets the world transform of this model mesh.
        /// </summary>
        public Matrix Transform
        {
            get { return worldTransform; }
        }
        internal Matrix worldTransform = Matrix.Identity;

        /// <summary>
        /// Gets or sets the material of this model mesh.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set { material = value; }
        }
        internal Material material;
        internal Material materialForRendering;

        /// <summary>
        /// Gets a collection containning all the materials used by this model mesh that are sorted based on level of detail.
        /// </summary>
        public MaterialLevelOfDetail MaterialLevels
        {
            get { return materialLevels; }
            set { materialLevels = value ?? new MaterialLevelOfDetail(); }
        }
        private MaterialLevelOfDetail materialLevels = new MaterialLevelOfDetail();

        /// <summary>
        /// Use the material from parent model if no materials are specified.
        /// </summary>
        Material IDrawableObject.Material
        {
            get { return materialForRendering; }
        }

        /// <summary>
        /// Gets or sets whether to use the default model diffuse texture, normal map, specular map, etc.
        /// Other than those specified in the material.
        /// The default behavior is to use the model default textures.
        /// </summary>
        public bool? UseModelTextures { get; set; }

        /// <summary>
        /// Gets the diffuse texture of this model mesh.
        /// </summary>
        public Texture2D Texture
        {
            get { return diffuseTexture; }
        }
        private Texture2D diffuseTexture;

        /// <summary>
        /// Gets the textures used by this model mesh based on texture usage.
        /// </summary>
        [Nine.Serialization.NotBinarySerializable]
        public IDictionary<TextureUsage, Texture> Textures
        {
            get { return textures; }
        }
        private Dictionary<TextureUsage, Texture> textures;
        
        IContainer IComponent.Parent
        {
            get { return model; }
            set { throw new InvalidOperationException(); }
        }
        #endregion

        #region Fields
        internal int parentBoneIndex;
        internal VertexBuffer vertexBuffer;
        internal IndexBuffer indexBuffer;
        internal int vertexOffset;
        internal int numVertices;
        internal int primitiveCount;
        internal int startIndex;

        private float distanceToCamera;
        #endregion

        #region ILightable
        bool IDrawableObject.CastShadow 
        {
            get { return model != null && model.CastShadow; } 
        }

        int ILightable.MaxReceivedShadows 
        { 
            get { return model != null ? model.MaxReceivedShadows : 0; } 
        }

        int ILightable.MaxAffectingLights 
        {
            get { return model != null ? model.MaxAffectingLights : 0; }
        }

        bool ILightable.MultiPassLightingEnabled 
        { 
            get { return model != null && model.MultiPassLightingEnabled; } 
        }

        bool ILightable.MultiPassShadowEnabled
        { 
            get { return model != null && model.MultiPassShadowEnabled; }
        }

        object ILightable.LightingData { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMesh"/> class.
        /// </summary>
        internal ModelMesh() { }

        /// <summary>
        /// Attaches this <see cref="ModelMesh"/> to a parent <see cref="model"/>.
        /// </summary>
        internal void Attach(Model model, Microsoft.Xna.Framework.Graphics.ModelMesh mesh, Microsoft.Xna.Framework.Graphics.ModelMeshPart part)
        {
            if (model == null || mesh == null || part == null)
                throw new ArgumentNullException();

            this.model = model;
            this.name = mesh.Name;

            var tag = part.Tag as ModelMeshPartTag;
            if (tag != null && tag.Textures != null && tag.Textures.Count > 0)
                this.textures = tag.Textures;
            this.diffuseTexture = part.Effect.GetTexture();
            this.parentBoneIndex = mesh.ParentBone.Index;
            this.vertexBuffer = part.VertexBuffer;
            this.indexBuffer = part.IndexBuffer;
            this.vertexOffset = part.VertexOffset;
            this.numVertices = part.NumVertices;
            this.primitiveCount = part.PrimitiveCount;
            this.startIndex = part.StartIndex;
        }
        #endregion

        #region Draw
        /// <summary>
        /// Perform any updates when this object has entered the main view frustum
        /// </summary>
        public bool OnAddedToView(DrawingContext context)
        {
            if (!(visible && model != null && model.visible))
                return false;

            model.insideViewFrustum = true;

            var xx = (context.matrices.cameraPosition.X - worldTransform.M41);
            var yy = (context.matrices.cameraPosition.Y - worldTransform.M42);
            var zz = (context.matrices.cameraPosition.Z - worldTransform.M43);

            distanceToCamera = (float)Math.Sqrt(xx * xx + yy * yy + zz * zz);
            
            materialForRendering = material ?? materialLevels.UpdateLevelOfDetail(distanceToCamera) ??
                             model.material ?? model.MaterialLevels.UpdateLevelOfDetail(distanceToCamera);
            return true;
        }

        /// <summary>
        /// Gets the distance from the position of the object to the current camera.
        /// </summary>
        public float GetDistanceToCamera(ref Vector3 cameraPosition)
        {
            // This method will always be called after OnAddedToView, so just use the result
            // calculated above.
            //
            // TODO: we are just using the orign of the model, it isn't always the right distance
            //       to sort transparency. Maybe the center of the bounding box or the nearest point
            //       to the camera should be used.
            return distanceToCamera;
        }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
            var graphics = context.graphics;
            var applyTexture = UseModelTextures.HasValue ? UseModelTextures.Value : model.UseModelTextures;
            
            if (applyTexture)
                ApplyTextures(material);
            ApplySkinTransform(material);

            material.world = worldTransform;
            material.BeginApply(context);

            // SetVertexBuffer isn't doing a good job filtering out same vertex buffer due to
            // multiple vertex buffer binding. Doing it manually here.
            context.SetVertexBuffer(vertexBuffer, vertexOffset);

            // On contrast, setting indices has no overhead for the same index buffers.
            graphics.Indices = indexBuffer;
            graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, startIndex, primitiveCount);
            
            material.EndApply(context);

            // The input material might be shared between different objects,
            // some of which might not even have a texture.
            // Since we have modified the material texture, we need to store it. 
            // 
            // We also modified other textures like normal maps, but these textures
            // aren't likely to be used by other object types, so don't restore them.
            if (applyTexture)
                material.texture = null;
        }

        internal void ApplyTextures(Material material)
        {
            material.texture = diffuseTexture;
            if (textures != null)
            {
                foreach (var pair in textures)
                    material.SetTexture(pair.Key, pair.Value);
            }
        }

        internal bool ApplySkinTransform(Material material)
        {
            var skinned = material.Find<IEffectSkinned>();
            if (skinned != null && (skinned.SkinningEnabled = model.IsSkinned))
            {
                var skinningEnabled = skinned.SkinningEnabled;
                if (skinningEnabled)
                    skinned.SetBoneTransforms(model.SkinTransforms);
                return skinningEnabled;
            }
            return false;
        }
        #endregion
    }
}