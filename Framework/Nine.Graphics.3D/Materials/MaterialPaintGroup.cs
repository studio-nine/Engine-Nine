namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a material part that is a layer of a material group.
    /// </summary>
    [ContentProperty("MaterialParts")]
    public class MaterialPaintGroup : MaterialPart
    {
        /// <summary>
        /// Gets a collection containing all the material parts use by this paint group.
        /// </summary>
        public IList<MaterialPart> MaterialParts
        {
            get { return materialParts; }
        }
        private MaterialPartCollection materialParts;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialPaintGroup"/> class.
        /// </summary>
        internal MaterialPaintGroup()
        {
            materialParts = new MaterialPartCollection();
        }

        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal override void OnBind()
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].ParameterSuffix = string.Concat(materialParts[i].ParameterSuffix, ParameterSuffix);

            materialParts.Bind(MaterialGroup);
        }

        /// <summary>
        /// Puts the dependent parts into the result list.
        /// </summary>
        protected internal override void GetDependentParts(MaterialUsage usage, IList<Type> result)
        {
            result.Add(typeof(MaterialParts.BeginPaintGroupMaterialPart));
            result.Add(typeof(MaterialParts.EndPaintGroupMaterialPart));

            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].GetDependentParts(usage, result);
        }

        /// <summary>
        /// Applies all the global shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void ApplyGlobalParameters(DrawingContext3D context)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].ApplyGlobalParameters(context);
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext3D context, MaterialGroup material)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].BeginApplyLocalParameters(context, material);
        }

        /// <summary>
        /// Restores any local shader parameters changes after drawing the promitive.
        /// </summary>
        protected internal override void EndApplyLocalParameters(DrawingContext3D context)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].EndApplyLocalParameters(context);
        }

        /// <summary>
        /// Gets the material with the specified usage that is attached to this material.
        /// </summary>
        protected internal override void OnResolveMaterialPart(MaterialUsage usage, MaterialPart existingInstance)
        {
            var result = ((MaterialPaintGroup)existingInstance);
            var srcCount = materialParts.Count;
            var destCount = result.materialParts.Count;
            if (srcCount > 0 && destCount > 0)
            {
                var src = 0;
                var dest = 0;
                var srcPart = materialParts[0];
                var destPart = result.materialParts[0];
                var srcType = srcPart.GetType();
                var destType = destPart.GetType();

                // Source material parts is a super set of destination material parts.
                while (dest < destCount)
                {
                    destPart = result.materialParts[dest++];
                    destType = destPart.GetType();

                    while (src < srcCount)
                    {
                        srcPart = materialParts[src++];
                        srcType = srcPart.GetType();
                        if (srcType == destType)
                        {
                            srcPart.OnResolveMaterialPart(usage, destPart);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the texture based on the texture usage.
        /// </summary>
        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            var count = materialParts.Count;
            for (int i = 0; i < count; ++i)
                materialParts[i].SetTexture(textureUsage, texture);
        }

        /// <summary>
        /// Gets the shader code for this material part based on material usage.
        /// </summary>
        protected internal override string GetShaderCode(MaterialUsage usage)
        {
#if WINDOWS
            if (!IsContentBuild)
                throw new InvalidOperationException();

            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            var pipelineAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == "Nine.Serialization");
            return (string)pipelineAssembly.GetTypes().Single(type => type.Name == "MaterialPaintGroupBuilder").InvokeMember("Build", flags, null, null, new object[] { this, usage });
#else
            throw new NotSupportedException();
#endif
        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// </summary>
        protected internal override MaterialPart Clone()
        {
            var result = new MaterialPaintGroup();
            for (int i = 0; i < materialParts.Count; ++i)
                result.MaterialParts.Add(materialParts[i].Clone());
            return result;
        }

        #region MaskTextureScale
        private static AttachableMemberIdentifier MaskTextureScaleProperty = new AttachableMemberIdentifier(typeof(MaterialPaintGroup), "MaskTextureScale");

        /// <summary>
        /// Gets the mask texture scale of the material group.
        /// </summary>
        public static Vector2 GetMaskTextureScale(MaterialGroup materialGroup)
        {
            Vector2 value = Vector2.One;
            AttachablePropertyServices.TryGetProperty(materialGroup, MaskTextureScaleProperty, out value);
            return value;
        }

        /// <summary>
        /// Sets the mask texture scale of the material group.
        /// </summary>
        public static void SetMaskTextureScale(MaterialGroup materialGroup, Vector2 value)
        {
            AttachablePropertyServices.SetProperty(materialGroup, MaskTextureScaleProperty, value);
        }
        #endregion

        #region MaskTextures
        private static AttachableMemberIdentifier MaskTexturesProperty = new AttachableMemberIdentifier(typeof(MaterialPaintGroup), "MaskTextures");

        /// <summary>
        /// Gets a list of mask textures of the material group.
        /// </summary>
        public static System.Collections.IList GetMaskTextures(MaterialGroup materialGroup)
        {
            System.Collections.IList value;
            if (!AttachablePropertyServices.TryGetProperty(materialGroup, MaskTexturesProperty, out value))
                AttachablePropertyServices.SetProperty(materialGroup, MaskTexturesProperty, value = new List<object>());
            return value;
        }

        /// <summary>
        /// Sets the mask textures of the material group.
        /// </summary>
        public static void SetMaskTextures(MaterialGroup materialGroup, System.Collections.IList value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            AttachablePropertyServices.SetProperty(materialGroup, MaskTexturesProperty, value);
        }
        #endregion
    }
}