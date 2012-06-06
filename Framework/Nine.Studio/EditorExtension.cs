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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio
{
    /// <summary>
    /// Represents extensions to the editor.
    /// </summary>
    public class EditorExtension : IDisposable
    {
        /// <summary>
        /// Gets or sets all document types.
        /// </summary>
        [ImportMany()]
        public IEnumerable<IDocumentType> DocumentTypes 
        {
            get { return documentTypes ?? new IDocumentType[0]; }
            set { documentTypes = value; }
        }
        private IEnumerable<IDocumentType> documentTypes;

        /// <summary>
        /// Gets or sets all object visualizers
        /// </summary>
        [ImportMany()]
        public IEnumerable<IDocumentVisualizer> DocumentVisualizers
        {
            get { return documentVisualizers ?? new IDocumentVisualizer[0]; }
            set { documentVisualizers = value; }
        }
        private IEnumerable<IDocumentVisualizer> documentVisualizers;

        /// <summary>
        /// Gets or sets all document serializers.
        /// </summary>
        [ImportMany()]
        public IEnumerable<IDocumentSerializer> DocumentSerializers
        {
            get { return documentSerializers ?? new IDocumentSerializer[0]; }
            set { documentSerializers = value; }
        }
        private IEnumerable<IDocumentSerializer> documentSerializers;

        /// <summary>
        /// Gets the composition container used to compose all extensions.
        /// </summary>
        private CompositionContainer Container;

        /// <summary>
        /// Initializes a new instance of EditorExtension.
        /// </summary>
        public EditorExtension()
        {
            Container = new CompositionContainer();
        }

        /// <summary>
        /// Loads the default extensions.
        /// </summary>
        public void LoadDefault()
        {
            if (Directory.Exists(Global.ExtensionDirectory))
                Container = new CompositionContainer(new DirectoryCatalog(Global.ExtensionDirectory));
            else
                Container = new CompositionContainer();
            Container.ComposeParts(this);

            Trace.TraceInformation("Extension Composed.");
        }

        /// <summary>
        /// Finds the first or default visualizer of the specified type.
        /// </summary>
        public IDocumentVisualizer FindVisualizer(Type targetType)
        {
            return DocumentVisualizers.FirstOrDefault(item => item.TargetType.IsAssignableFrom(targetType));
        }

        /// <summary>
        /// Finds all the visualizer of the specified type.
        /// </summary>
        public IEnumerable<IDocumentVisualizer> FindVisualizers(Type targetType)
        {
            return DocumentVisualizers.Where(item => item.TargetType.IsAssignableFrom(targetType));
        }

        /// <summary>
        /// Finds the first or default serializer of the specified type.
        /// </summary>
        public IDocumentSerializer FindSerializer(Type targetType)
        {
            return DocumentSerializers.FirstOrDefault(item => item.TargetType.IsAssignableFrom(targetType));
        }

        /// <summary>
        /// Finds all the visualizer of the serializer type.
        /// </summary>
        public IEnumerable<IDocumentSerializer> FindSerializers(Type targetType)
        {
            return DocumentSerializers.Where(item => item.TargetType.IsAssignableFrom(targetType));
        }

        /// <summary>
        /// Finds all the importers
        /// </summary>
        public IEnumerable<IDocumentSerializer> FindImporters()
        {
            return DocumentSerializers.Where(s => s.CanDeserialize && s.FileExtensions != null && s.FileExtensions.Count() > 0);
        }

        /// <summary>
        /// Finds all the exporters
        /// </summary>
        public IEnumerable<IDocumentSerializer> FindExporters()
        {
            return DocumentSerializers.Where(s => s.CanSerialize && s.FileExtensions != null && s.FileExtensions.Count() > 0);
        }

        public void Dispose()
        {
            if (Container != null)
                Container.Dispose();
        }
    }
}
