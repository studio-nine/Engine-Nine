#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics;

#endregion

namespace Nine
{
    #region ITemplateFactory
    /// <summary>
    /// Defines a template factory that can create a template of the specified
    /// type based on the name of the template.
    /// </summary>
    public interface ITemplateFactory
    {
        /// <summary>
        /// Gets the name of this template factory.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Creates a new instance of the target type using the specified template name.
        /// </summary>
        object Create(Type targetType, string templateName);
    }
    #endregion

    #region ClassTemplateFactory
    /// <summary>
    /// Defines a template factory that can create templates from existing class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ClassTemplateFactory : ITemplateFactory
    {
        /// <summary>
        /// Gets the name of this template factory.
        /// </summary>
        public string Name { get { return "Class"; } }

        /// <summary>
        /// Creates a new instance of the target type using the specified template name.
        /// </summary>
        public virtual object Create(Type targetType, string templateName)
        {
            return Activator.CreateInstance(Type.GetType(templateName));
        }
    }
    #endregion

    #region ContentTemplateFactory
    /// <summary>
    /// Defines a template factory that can create templates using content pipeline.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ContentTemplateFactory : ITemplateFactory, IDisposable
    {
        /// <summary>
        /// Gets the name of this template factory.
        /// </summary>
        public string Name { get { return "Content"; } }

        /// <summary>
        /// Gets the underlying content manager used by this template factory.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Initializes a new instance of TemplateFactory.
        /// </summary>
        public ContentTemplateFactory(IServiceProvider serviceProvider)
        {
            Content = new TemplateFactoryContentManager(serviceProvider);
            Content.RootDirectory = "Content";
        }
        
#if !WINDOWS_PHONE
        /// <summary>
        /// Initializes a new instance of TemplateFactory.
        /// </summary>
        public ContentTemplateFactory(ContentManager contentManager)
        {
            if (contentManager == null)
                throw new ArgumentNullException("contentManager");

            Content = contentManager;
            needReflection = true;
        }

        bool needReflection;
        static Type[] ReadAssetParameterTypes = new Type[] { typeof(string), typeof(Action<IDisposable>) };
#endif

        /// <summary>
        /// Creates a new instance of the target type using the specified template name.
        /// </summary>
        public virtual object Create(Type targetType, string templateName)
        {
#if WINDOWS_PHONE
            return Content.Load<object>(templateName);
#else
            if (!needReflection)
            {
                try
                {
                    ((TemplateFactoryContentManager)Content).IsReadAsset = true;
                    return Content.Load<object>(templateName);
                }
                finally
                {
                    ((TemplateFactoryContentManager)Content).IsReadAsset = false;
                }
            }

            // Hack into ReadAsset using reflection.
            var readAsset = Content.GetType().GetMethod("ReadAsset", BindingFlags.Instance | BindingFlags.NonPublic, null,
                                                        ReadAssetParameterTypes, null);

            try
            {
                return readAsset.Invoke(Content, new object[] { templateName, null });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
#endif
        }

        public void Dispose()
        {
            if (Content != null)
                Content.Dispose();
        }
    }

    /// <summary>
    /// Defines a content manager that loads a new instance for the same asset name.
    /// </summary>
    class TemplateFactoryContentManager : ContentManager
    {
        internal bool IsReadAsset;

        public TemplateFactoryContentManager(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        public override T Load<T>(string assetName)
        {
            if (IsReadAsset)
                return base.ReadAsset<T>(assetName, null);
            return base.Load<T>(assetName);
        }
    }
    #endregion
}