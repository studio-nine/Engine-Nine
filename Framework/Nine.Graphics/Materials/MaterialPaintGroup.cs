#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
#endregion

namespace Nine.Graphics.Materials
{
    /// <summary>
    /// Defines a material part that is a layer of a material group.
    /// </summary>
    [ContentProperty("MaterialParts")]
    public class MaterialPaintGroup : MaterialPart
    {
        /// <summary>
        /// Gets or sets the mask texture used to splat this paint group.
        /// </summary>
        public Texture2D MaskTexture { get; set; }

        /// <summary>
        /// Gets or sets the color channel of the above mask texture.
        /// Values 0, 1, 2, 3 represents r, g, b, a channels respectively.
        /// </summary>
        public int MaskTextureChannel { get; set; }

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
            materialParts.Bind(MaterialGroup);
        }

        /// <summary>
        /// Gets the shader code for this material part based on material usage.
        /// </summary>
        protected internal override string GetShaderCode(MaterialUsage usage)
        {
#if WINDOWS
            if (!IsContentBuild)
                throw new InvalidOperationException();
            
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            return (string)(Type.GetType("Nine.Content.Pipeline.Graphics.Materials.MaterialPaintGroupBuilder, Nine.Content.Pipeline", true)
                                .InvokeMember("Build", flags, null, null, new object[] { this, usage }));
#else
            throw new NotSupportedException();
#endif
        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// </summary>
        protected internal override MaterialPart Clone()
        {
            return new MaterialPaintGroup();
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
        private static AttachableMemberIdentifier MaskTexturesProperty = new AttachableMemberIdentifier(typeof(MaterialPaintGroup), "MaskTexturesProperty");

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