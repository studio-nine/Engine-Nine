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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio
{
    using Exporter = Lazy<IExporter,IMetadata>;
    using Factory = Lazy<IFactory,IMetadata>;
    using Importer = Lazy<IImporter,IMetadata>;
    using Settings = Lazy<ISettings,IMetadata>;
    using Tool = Lazy<ITool,IMetadata>;
    using Visualizer = Lazy<IVisualizer,IMetadata>;

    /// <summary>
    /// Represents extensions to the editor.
    /// </summary>
    public class EditorExtensions : IDisposable
    {
        /// <summary>
        /// Gets or sets all document factories.
        /// </summary>
        [ImportMany()]
        public IEnumerable<Factory> Factories
        {
            get { return factories ?? Enumerable.Empty<Factory>(); }
            set { factories = value; }
        }
        private IEnumerable<Factory> factories;

        /// <summary>
        /// Gets or sets all object visualizers
        /// </summary>
        [ImportMany()]
        public IEnumerable<Visualizer> Visualizers
        {
            get { return visualizers ?? Enumerable.Empty<Visualizer>(); }
            set { visualizers = value; }
        }
        private IEnumerable<Visualizer> visualizers;

        /// <summary>
        /// Gets or sets all object designers
        /// </summary>
        [ImportMany()]
        public IEnumerable<Tool> Tools
        {
            get { return tools ?? Enumerable.Empty<Tool>(); }
            set { tools = value; }
        }
        private IEnumerable<Tool> tools;

        /// <summary>
        /// Gets or sets all document importers.
        /// </summary>
        [ImportMany()]
        public IEnumerable<Importer> Importers
        {
            get { return importers ?? Enumerable.Empty<Importer>(); }
            set { importers = value; }
        }
        private IEnumerable<Importer> importers;

        /// <summary>
        /// Gets or sets all document exporters.
        /// </summary>
        [ImportMany()]
        public IEnumerable<Exporter> Exporters
        {
            get { return exporters ?? Enumerable.Empty<Exporter>(); }
            set { exporters = value; }
        }
        private IEnumerable<Exporter> exporters;

        /// <summary>
        /// Gets or sets all document settings.
        /// </summary>
        [ImportMany()]
        public IEnumerable<Settings> Settings
        {
            get { return settings ?? Enumerable.Empty<Settings>(); }
            set { settings = value; }
        }
        private IEnumerable<Settings> settings;

        /// <summary>
        /// Gets or sets all attribute providers.
        /// </summary>
        [ImportMany()]
        public IEnumerable<IAttributeProvider> AttributeProviders
        {
            get { return attributeProviders ?? new IAttributeProvider[0]; }
            set { attributeProviders = value; /* TypeDescriptorHelper.AddAttributes(value); */ }
        }
        private IEnumerable<IAttributeProvider> attributeProviders;

        /// <summary>
        /// Gets the composition container used to compose all extensions.
        /// </summary>
        private CompositionContainer Container;

        /// <summary>
        /// Initializes a new instance of EditorExtension.
        /// </summary>
        public EditorExtensions()
        {
            Container = new CompositionContainer();
        }

        /// <summary>
        /// Loads the default extensions.
        /// </summary>
        public void LoadDefault()
        {
            Trace.TraceInformation("Loading default editor extensions at folder {0}", Path.GetFullPath(Constants.ExtensionDirectory));

            if (Directory.Exists(Constants.ExtensionDirectory))
                Container = new CompositionContainer(new DirectoryCatalog(Constants.ExtensionDirectory));
            else
                Container = new CompositionContainer();

            try
            {
                Container.ComposeParts(this);
            }
            catch (ReflectionTypeLoadException e)
            {
                Trace.TraceError("Error loading assembly");
                Trace.TraceError(e.ToString());
                foreach (var loaderException in e.LoaderExceptions)
                    Trace.TraceError(loaderException.ToString());
            }

            TraceExtensions();
            Trace.TraceInformation("Extension Composed.");
        }

        private void TraceExtensions()
        {
            Trace.TraceInformation("Factories {0}:", Factories.Count());
            Factories.ForEach(x => Trace.TraceInformation("{0} - {1}", x.Value.GetType().AssemblyQualifiedName, GetMetadataString(x.Metadata)));

            Trace.TraceInformation("Importers {0}:", Importers.Count());
            Importers.ForEach(x => Trace.TraceInformation("{0} - {1}", x.Value.GetType().AssemblyQualifiedName, GetMetadataString(x.Metadata)));

            Trace.TraceInformation("Exporters: {0}", Exporters.Count());
            Exporters.ForEach(x => Trace.TraceInformation("{0} - {1}", x.Value.GetType().AssemblyQualifiedName, GetMetadataString(x.Metadata)));

            Trace.TraceInformation("Visualizers: {0}", Visualizers.Count());
            Visualizers.ForEach(x => Trace.TraceInformation("{0} - {1}", x.Value.GetType().AssemblyQualifiedName, GetMetadataString(x.Metadata)));

            Trace.TraceInformation("Settings: {0}", Settings.Count());
            Settings.ForEach(x => Trace.TraceInformation("{0} - {1}", x.Value.GetType().AssemblyQualifiedName, GetMetadataString(x.Metadata)));

            Trace.TraceInformation("AttributeProviders: {0}", AttributeProviders.Count());
            AttributeProviders.ForEach(x => Trace.TraceInformation(x.GetType().AssemblyQualifiedName));
        }

        private string GetMetadataString(IMetadata metadata)
        {
            return string.Format("[{0}][{1}]", metadata.DisplayName ?? "", metadata.Category ?? "");
        }

        /// <summary>
        /// Finds the default exporter for the given object type.
        /// </summary>
        public Exporter FindExporter(Type type)
        {
            return Exporters.SingleOrDefault(e => e.Metadata.IsDefault && e.Value.TargetType.IsAssignableFrom(type))
                ?? Exporters.FirstOrDefault(e => e.Value.TargetType.IsAssignableFrom(type));
        }

        /// <summary>
        /// Finds the default importer for the given object type.
        /// </summary>
        public Importer FindImporter(Type type)
        {
            return Importers.SingleOrDefault(e => e.Metadata.IsDefault && e.Value.TargetType.IsAssignableFrom(type))
                ?? Importers.FirstOrDefault(e => e.Value.TargetType.IsAssignableFrom(type));
        }
        
        /// <summary>
        /// Finds the default visualizer for the given object type.
        /// </summary>
        public Visualizer FindVisualizer(Type type)
        {
            return Visualizers.SingleOrDefault(e => e.Metadata.IsDefault && e.Value.TargetType.IsAssignableFrom(type))
                ?? Visualizers.FirstOrDefault(e => e.Value.TargetType.IsAssignableFrom(type));
        }
        
        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        public IEnumerable<Attribute> GetCustomAttribute(Type type)
        {
            return AttributeProviders.SelectMany(ap => ap.GetCustomAttributes(type));
        }

        /// <summary>
        /// Gets the custom attribute.
        /// </summary>
        public IEnumerable<Attribute> GetCustomAttribute(Type type, string member)
        {
            return AttributeProviders.SelectMany(ap => ap.GetCustomAttributes(type, member));
        }

        public void Dispose()
        {
            if (Container != null)
                Container.Dispose();
        }
    }
}
