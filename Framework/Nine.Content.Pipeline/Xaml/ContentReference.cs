#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Content.Pipeline.Xaml;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    /// <summary>
    /// Defines an interface to resolve content references.
    /// </summary>
    internal interface IContentReferenceProvider
    {
        object ResolveContentReference(string name);
    }

    /// <summary>
    /// Defines a markup extension to reference external content.
    /// </summary>
    [ContentProperty("AssetNameName")]
    public class ContentReference : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the name of the content.
        /// This value should be set to the asset name instead of filename when used with Xna content pipeline.
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        public ContentReference() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        /// <param name="assetName">The name of the asset.</param>
        public ContentReference(string assetName) { AssetName = assetName; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var externalReferenceProvider = serviceProvider.GetService<IContentReferenceProvider>();
            if (externalReferenceProvider != null)
                return externalReferenceProvider.ResolveContentReference(AssetName);
            return null;
        }
    }
}
