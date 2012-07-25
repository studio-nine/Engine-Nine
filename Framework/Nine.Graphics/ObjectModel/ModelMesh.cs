namespace Nine.Graphics.ObjectModel
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Defines a part of a model that contains only one material.
    /// </summary>
    [ContentSerializable]
    public class ModelMesh : IDrawableObject, ILightable, IContainedObject
    {
        #region Properties
        /// <summary>
        /// Gets the name of this model part.
        /// </summary>
        public string Name
        {
            get { return name; } 
        }
        private string name;

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
        private Matrix worldTransform = Matrix.Identity;

        /// <summary>
        /// Visibility can be controlled by both ModelPart and Model.
        /// </summary>
        bool IDrawableObject.Visible 
        {
            get { return visible && model != null && model.visible; } 
        }

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
        [ContentSerializerIgnore]
        public IDictionary<TextureUsage, Texture> Textures
        {
            get { return textures; }
        }
        private Dictionary<TextureUsage, Texture> textures;
        
        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        
        object IContainedObject.Parent
        {
            get { return model; }
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
        #endregion

        #region ILightable
        bool ILightable.CastShadow 
        {
            get { return Model != null && Model.CastShadow; } 
        }

        bool ILightable.ReceiveShadow
        {
            get { return Model != null && Model.ReceiveShadow; } 
        }

        bool ILightable.LightingEnabled 
        {
            get { return Model != null && Model.LightingEnabled; }
        }

        int ILightable.MaxReceivedShadows 
        { 
            get { return Model != null ? Model.MaxReceivedShadows : 0; } 
        }

        int ILightable.MaxAffectingLights 
        {
            get { return Model != null ? Model.MaxAffectingLights : 0; }
        }

        bool ILightable.MultiPassLightingEnabled 
        { 
            get { return Model != null && Model.MultiPassLightingEnabled; } 
        }

        bool ILightable.MultiPassShadowEnabled
        { 
            get { return Model != null && Model.MultiPassShadowEnabled; }
        }

        object ILightable.LightingData { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelMesh"/> class.
        /// </summary>
        internal ModelMesh() { }

        /// <summary>
        /// Attaches this <see cref="ModelMesh"/> to a parent <see cref="Model"/>.
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
        /// Perform any updates before this object is drawed.
        /// </summary>
        public void BeginDraw(DrawingContext context)
        {
            model.insideViewFrustum = true;
            model.UpdateBoneTransforms();

            // Manually do some optimization here.
            if (model.isAbsoluteTransformDirty)
            {
                Matrix absoluteTransform = model.AbsoluteTransform;
                Matrix.Multiply(ref model.BoneTransforms[parentBoneIndex], ref absoluteTransform, out worldTransform);
            }
            else
            {
                Matrix.Multiply(ref model.BoneTransforms[parentBoneIndex], ref model.absoluteTransform, out worldTransform);
            }

            if ((materialForRendering = material) == null)
            {
                Vector3 position = new Vector3();
                position.X = worldTransform.M41;
                position.Y = worldTransform.M42;
                position.Z = worldTransform.M43;

                float distanceToEye;
                Vector3.Distance(ref context.matrices.eyePosition, ref position, out distanceToEye);

                materialForRendering = materialLevels.UpdateLevelOfDetail(distanceToEye) ??
                    model.material ?? model.MaterialLevels.UpdateLevelOfDetail(distanceToEye);
            }
        }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
            var graphics = context.GraphicsDevice;

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
        }

        internal void ApplyTextures(Material material)
        {
            if (material != this.material &&
               (UseModelTextures ?? model.UseModelTextures))
            {
                material.Texture = diffuseTexture;
                if (textures != null)
                {
                    foreach (var pair in textures)
                        material.SetTexture(pair.Key, pair.Value);
                }
            }
        }

        internal bool ApplySkinTransform(Material material)
        {
            var skinned = material.Find<IEffectSkinned>();
            if (skinned != null && (skinned.SkinningEnabled = Model.IsSkinned))
            {
                var skinningEnabled = skinned.SkinningEnabled;
                if (skinningEnabled)
                    skinned.SetBoneTransforms(Model.SkinTransforms);
                return skinningEnabled;
            }
            return false;
        }

        void IDrawableObject.EndDraw(DrawingContext context) 
        {

        }
        #endregion
    }
}