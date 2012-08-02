namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Represents a basic building block of a material group.
    /// </summary>
    [RuntimeNameProperty("Name")]
    [DictionaryKeyProperty("Name")]
    public abstract class MaterialPart
    {
        [ContentSerializer]
        internal string ParameterSuffix;

        /// <summary>
        /// Gets or sets the name of this material part.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the EffectParts from the parent LinkedEffectContent.
        /// </summary>
        public MaterialGroup MaterialGroup { get; internal set; }

        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal abstract void OnBind();
        
        /// <summary>
        /// Gets the shader code for this material part based on material usage.
        /// </summary>
        protected internal abstract string GetShaderCode(MaterialUsage usage);

        /// <summary>
        /// Puts the dependent parts into the result list.
        /// </summary>
        protected internal virtual void GetDependentParts(MaterialUsage usage, IList<Type> result) { }

        /// <summary>
        /// Puts the dependent textures into the result list.
        /// </summary>
        protected internal virtual void GetDependentPasses(ICollection<Type> passTypes) { }

        /// <summary>
        /// Sets the texture based on the texture usage.
        /// </summary>
        public virtual void SetTexture(TextureUsage textureUsage, Texture texture) { }

        /// <summary>
        /// Applies all the global shader parameters before drawing any primitives.
        /// </summary>
        protected internal virtual void ApplyGlobalParameters(DrawingContext context) { }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal virtual void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material) { }

        /// <summary>
        /// Restores any local shader parameters changes after drawing the promitive.
        /// </summary>
        protected internal virtual void EndApplyLocalParameters() { }

        /// <summary>
        /// Gets the material with the specified usage that is attached to this material.
        /// </summary>
        protected internal virtual void OnResolveMaterialPart(MaterialUsage usage, MaterialPart existingInstance) { }

        /// <summary>
        /// Notifies the material system that the shader has changed and needs recompile.
        /// </summary>
        protected void NotifyShaderChanged()
        {
#if WINDOWS
            if (!MaterialPart.IsContentBuild && !MaterialPart.IsContentRead)
                MaterialGroup.OnShaderChanged();
#endif
        }

        /// <summary>
        /// Copies data from an existing object to this object. 
        /// </summary>
        protected internal abstract MaterialPart Clone();

        /// <summary>
        /// Gets the EffectParameter with the name from the fragment parameter name.
        /// </summary>
        protected EffectParameter GetParameter(string name)
        {
            if (IsContentBuild)
                return null;
            return MaterialGroup.Effect.Parameters[string.Concat(name, ParameterSuffix)];
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the EffectParameter by semantic from the fragment parameter name.
        /// </summary>
        protected EffectParameter GetParameterBySemantic(string semantic)
        {
            if (IsContentBuild)
                return null;

            foreach (EffectParameter parameter in MaterialGroup.Effect.Parameters)
                if (parameter.Semantic == semantic && parameter.Name.EndsWith(ParameterSuffix))
                    return parameter;
            return null;
        }
#endif

        #region GetShaderCode
        static MaterialPart()
        {
            var pipelineAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "Nine.Content.Pipeline");
            if (pipelineAssembly != null)
            {
                ShaderResources = new global::System.Resources.ResourceManager("Nine.Content.Pipeline.Graphics.Materials.MaterialPartShaders", pipelineAssembly);
                IsContentBuild = true;
            }
        }

        /// <summary>
        /// Gets the shader coder from the content pipeline assembly.
        /// </summary>
        internal static string GetShaderCode(string resourceKey)
        {
#if WINDOWS
            if (!IsContentBuild)
                throw new InvalidOperationException();
            return System.Text.Encoding.UTF8.GetString((byte[])ShaderResources.GetObject(resourceKey));
#else
            throw new NotSupportedException();
#endif
        }
#if WINDOWS
        private static System.Resources.ResourceManager ShaderResources;
        internal static bool IsContentBuild = false;
        internal static bool IsContentRead = false;
#endif
        #endregion
    }
}