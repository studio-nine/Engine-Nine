namespace Nine.Design
{
    using System.ComponentModel.Composition;
    using Nine.Studio.Extensibility;
    using Nine.Studio.Serializers;

    [Export(typeof(IFactory))]
    [LocalizedCategory("General")]
    [LocalizedDisplayName("World", typeof(Resources))]
    public class WorldFactory : Factory<World, object> { }

    [Default()]
    [Export(typeof(IExporter))]
    [Export(typeof(IImporter))]
    [LocalizedCategory("General")]
    [LocalizedDisplayName("World", typeof(Resources))]
    public class WorldSerializer : XamlSerializer { }

    [Export(typeof(IAttributeProvider))]
    public class WorldAttributeProvider : AttributeProvider<World>
    {
        public WorldAttributeProvider()
        {
            AddCustomAttribute(new LocalizedCategoryAttribute("General"));
            AddCustomAttribute(new LocalizedDisplayNameAttribute("World", typeof(Resources)));
            AddCustomAttribute(new FolderNameAttribute("Worlds"));
        }
    }
}
