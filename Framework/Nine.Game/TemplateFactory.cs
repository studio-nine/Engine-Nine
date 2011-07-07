#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics;
using Nine.Graphics.Views;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using System.Text;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a template factory that can create a template of the specified
    /// type based on the name of the template.
    /// </summary>
    public interface ITemplateFactory
    {
        /// <summary>
        /// Creates a new instance of the target type using the specified template name.
        /// </summary>
        object Create(Type targetType, string templateName);
    }

    /// <summary>
    /// Defines a default template factory that can creates a template from
    /// content pipeline.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TemplateFactory : ITemplateFactory
    {
        /// <summary>
        /// Gets the underlying content manager used by this template factory.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of TemplateFactory.
        /// </summary>
        public TemplateFactory(IServiceProvider serviceProvider)
        {
            Content = new TemplateFactoryContentManager(serviceProvider);
        }

        /// <summary>
        /// Creates a new instance of the target type using the specified template name.
        /// </summary>
        public virtual object Create(Type targetType, string templateName)
        {
            return null;
        }
    }

    /// <summary>
    /// Defines a content manager that loads a new instance for the same asset name.
    /// </summary>
    class TemplateFactoryContentManager : ContentManager
    {
        static int Id = 0;

        public TemplateFactoryContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        public override T Load<T>(string assetName)
        {
            return base.Load<T>(assetName + ":" + Id.ToString());
        }

        protected override System.IO.Stream OpenStream(string assetName)
        {
            return base.OpenStream(assetName.Substring(0, assetName.LastIndexOf(':')));
        }
    }
}