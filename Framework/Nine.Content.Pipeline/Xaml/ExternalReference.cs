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
using System.Collections;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    /// <summary>
    /// Defines a markup extension to reference external source asset.
    /// </summary>
    [ContentProperty("FileName")]
    public class ExternalReference : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the file name of the source asset.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        public ExternalReference() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentReference"/> class.
        /// </summary>
        /// <param name="fileName">The name of the asset.</param>
        public ExternalReference(string fileName) { FileName = fileName; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(FileName))
                return null;
            
            Type destinationType = null;            
            var destinationTypeProvider = serviceProvider.GetService<IDestinationTypeProvider>();
            if (destinationTypeProvider != null)
            {
                try
                {
                    destinationType = destinationTypeProvider.GetDestinationType();
                }
                catch
                {
                    try
                    {
                        var provideValueTarget = serviceProvider.GetService<IProvideValueTarget>();
                        if (provideValueTarget != null && provideValueTarget.TargetObject != null)
                        {
                            if (provideValueTarget.TargetProperty is PropertyInfo)
                                destinationType = ((PropertyInfo)provideValueTarget.TargetProperty).PropertyType;
                            else
                                // Need to get destination type from the target object when it is a collection
                                destinationType = provideValueTarget.TargetObject.GetType().GetGenericArguments()[0];
                        }
                    }
                    catch
                    {
                        destinationType = null;
                    }
                }
            }

            try
            {
                return Activator.CreateInstance(destinationType, FileName);
            }
            catch
            {
                return null;
            }
        }
    }
}
