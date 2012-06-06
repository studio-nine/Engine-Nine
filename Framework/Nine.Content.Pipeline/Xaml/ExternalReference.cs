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
    /// Defines a markup extension to reference external source asset.
    /// </summary>
    public class ExternalReference : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the file name of the source asset.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the name of the result content or null to use the default value.
        /// </summary>
        public string AssetName { get; set; }

        /// <summary>
        /// Gets or sets the importer for the source asset.
        /// </summary>
        public IContentImporter Importer { get; set; }

        /// <summary>
        /// Gets or sets the processor for the source asset.
        /// </summary>
        public IContentProcessor Processor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        public ExternalReference() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        public ExternalReference(string name) { FileName = name; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            PipelineBuilder builder = new PipelineBuilder();
            builder.Build(FileName, Importer, Processor, AssetName);
            return null;
        }
    }
}
