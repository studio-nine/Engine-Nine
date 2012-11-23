namespace Nine.Studio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Nine.Studio.Extensibility;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents extensions to the editor.
    /// </summary>
    public class EditorExtensions
    {
        public ObservableCollection<Extension<IFactory>> Factories { get; private set; }
        public ObservableCollection<Extension<IVisualizer>> Visualizers { get; private set; }
        public ObservableCollection<Extension<ITool>> Tools { get; private set; }
        public ObservableCollection<Extension<IImporter>> Importers { get; private set; }
        public ObservableCollection<Extension<IExporter>> Exporters { get; private set; }
        public ObservableCollection<Extension<ISettings>> Settings { get; private set; }
        public ObservableCollection<IAttributeProvider> AttributeProviders { get; private set; }

        internal EditorExtensions()
        {
            Factories = new ObservableCollection<Extension<IFactory>>();
            Visualizers = new ObservableCollection<Extension<IVisualizer>>();
            Tools = new ObservableCollection<Extension<ITool>>();
            Importers = new ObservableCollection<Extension<IImporter>>();
            Exporters = new ObservableCollection<Extension<IExporter>>();
            Settings = new ObservableCollection<Extension<ISettings>>();
            AttributeProviders = new ObservableCollection<IAttributeProvider>();
            AttributeProviders.CollectionChanged += (sender, e) => { TypeDescriptorHelper.SetAttributeProviders(AttributeProviders); };
        }

        internal void Load(Editor editor, string path)
        {
            Verify.DirectoryExists(path, "path");

            Trace.TraceInformation("Loading default editor extensions at folder {0}", Path.GetFullPath(path));

            var extensions = Directory.GetFiles(path, "*.DLL").Concat(Directory.GetFiles(path, "*.EXE"))
                .SelectMany(file => LoadExportedTypes(file))
                .Select(type => new { Export = type.GetCustomAttributes(typeof(ExportAttribute), false).SingleOrDefault(), Type = type })
                .Where(pair => pair.Export != null)
                .ToLookup(pair => ((ExportAttribute)pair.Export).ContractType, pair => pair.Type);

            AttributeProviders.AddRange(from type in extensions[typeof(IAttributeProvider)] where !AttributeProviders.Any(x => x.GetType() == type) select (IAttributeProvider)Activator.CreateInstance(type));
            Factories.AddRange(from type in extensions[typeof(IFactory)] where !Factories.Any(x => x.Value.GetType() == type) select new Extension<IFactory>(editor, (IFactory)Activator.CreateInstance(type)));
            Visualizers.AddRange(from type in extensions[typeof(IVisualizer)] where !Visualizers.Any(x => x.Value.GetType() == type) select new Extension<IVisualizer>(editor, (IVisualizer)Activator.CreateInstance(type)));
            Tools.AddRange(from type in extensions[typeof(ITool)] where !Tools.Any(x => x.Value.GetType() == type) select new Extension<ITool>(editor, (ITool)Activator.CreateInstance(type)));
            Importers.AddRange(from type in extensions[typeof(IImporter)] where !Importers.Any(x => x.Value.GetType() == type) select new Extension<IImporter>(editor, (IImporter)Activator.CreateInstance(type)));
            Exporters.AddRange(from type in extensions[typeof(IExporter)] where !Exporters.Any(x => x.Value.GetType() == type) select new Extension<IExporter>(editor, (IExporter)Activator.CreateInstance(type)));
            Settings.AddRange(from type in extensions[typeof(ISettings)] where !Settings.Any(x => x.Value.GetType() == type) select new Extension<ISettings>(editor, (ISettings)Activator.CreateInstance(type)));

            TraceExtensions();

            Trace.TraceInformation("Extensions Loaded.");
        }

        private IEnumerable<Type> LoadExportedTypes(string file)
        {
            try
            {
                return Assembly.LoadFrom(file).GetTypes();
            }
            catch
            {
                Trace.TraceError("Error loading extension " + file);
                return Type.EmptyTypes;
            }
        }

        private void TraceExtensions()
        {
            Trace.TraceInformation("Factories {0}:", Factories.Count);
            Factories.ForEach(x => Trace.TraceInformation(x.ToString()));

            Trace.TraceInformation("Importers {0}:", Importers.Count);
            Importers.ForEach(x => Trace.TraceInformation(x.ToString()));

            Trace.TraceInformation("Exporters: {0}", Exporters.Count);
            Exporters.ForEach(x => Trace.TraceInformation(x.ToString()));

            Trace.TraceInformation("Visualizers: {0}", Visualizers.Count);
            Visualizers.ForEach(x => Trace.TraceInformation(x.ToString()));

            Trace.TraceInformation("Settings: {0}", Settings.Count);
            Settings.ForEach(x => Trace.TraceInformation(x.ToString()));

            Trace.TraceInformation("AttributeProviders: {0}", AttributeProviders.Count);
            AttributeProviders.ForEach(x => Trace.TraceInformation(x.ToString()));
        }

        internal Extension<IExporter> FindExporter(Type type)
        {
            return Exporters.SingleOrDefault(e => e.IsDefault && e.Value.TargetType == type)
                ?? Exporters.FirstOrDefault(e => e.Value.TargetType == type);
        }

        internal Extension<IImporter> FindImporter(Type type)
        {
            return Importers.SingleOrDefault(e => e.IsDefault && e.Value.TargetType == type)
                ?? Importers.FirstOrDefault(e => e.Value.TargetType == type);
        }

        internal Extension<IVisualizer> FindVisualizer(Type type)
        {
            return Visualizers.SingleOrDefault(e => e.IsDefault && e.Value.TargetType == type)
                ?? Visualizers.FirstOrDefault(e => e.Value.TargetType == type);
        }

        internal IEnumerable<Attribute> GetCustomAttributes(Type type)
        {
            return AttributeProviders.Where(ap => type.IsAssignableFrom(ap.TargetType)).SelectMany(ap => ap.GetCustomAttributes());
        }

        internal IEnumerable<Attribute> GetCustomAttributes(Type type, string member)
        {
            return AttributeProviders.Where(ap => type.IsAssignableFrom(ap.TargetType)).SelectMany(ap => ap.GetCustomAttributes(member));
        }

        internal string GetDisplayName(object value)
        {
            string result = null;
            var attributes = GetCustomAttributes(value.GetType()).Concat(value.GetType().GetCustomAttributes(true)).ToArray();
            foreach (var metadata in attributes.OfType<ExportMetadataAttribute>())
            {
                result = string.IsNullOrWhiteSpace(result) ? metadata.DisplayName : result;
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                foreach (var metadata in attributes.OfType<DisplayNameAttribute>())
                    result = string.IsNullOrWhiteSpace(result) ? metadata.DisplayName : result;
            }
            return string.IsNullOrWhiteSpace(result) ? Strings.NoName : result;
        }

        internal string GetCategory(object value)
        {
            string result = null;
            var attributes = GetCustomAttributes(value.GetType()).Concat(value.GetType().GetCustomAttributes(true)).ToArray();
            foreach (var metadata in attributes.OfType<ExportMetadataAttribute>())
            {
                result = string.IsNullOrWhiteSpace(result) ? metadata.Category : result;
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                foreach (var metadata in attributes.OfType<CategoryAttribute>())
                    result = string.IsNullOrWhiteSpace(result) ? metadata.Category : result;
            }
            return string.IsNullOrWhiteSpace(result) ? Strings.General : result;
        }

        internal string GetClass(object value)
        {
            string result = null;
            var attributes = GetCustomAttributes(value.GetType()).Concat(value.GetType().GetCustomAttributes(true)).ToArray();
            foreach (var metadata in attributes.OfType<ExportMetadataAttribute>())
            {
                result = string.IsNullOrWhiteSpace(result) ? metadata.Class : result;
            }
            return string.IsNullOrWhiteSpace(result) ? "Misc" : result;
        }
    }
}
