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
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Nine.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a world that contains objects to be updated and rendered.
    /// </summary>
    public class World : IUpdateable, IDrawable
    {
        /// <summary>
        /// Gets the template factory of this world.
        /// </summary>
        [XmlIgnore]
        public ICollection<ITemplateFactory> TemplateFactories { get; private set; }

        /// <summary>
        /// Gets the services owned by this world.
        /// </summary>
        [XmlIgnore]
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// Gets a collection of world objects managed by this world.
        /// </summary>
        [XmlArrayItem("WorldObject")]
        public WorldObjectCollection<object> WorldObjects
        {
            get { return worldObjects; } 
        }

        /// <summary>
        /// Optimize access to graphics context.
        /// </summary>
        internal Renderer Renderer;

        private TimeEventArgs timeEventArgs = new TimeEventArgs();
        private WorldObjectCollection<object> worldObjects = new WorldObjectCollection<object>();
        
        /// <summary>
        /// Occurs when the world is updating itself.
        /// </summary>
        public event EventHandler<TimeEventArgs> Updating;

        /// <summary>
        /// Occurs when the world is drawing itself.
        /// </summary>
        public event EventHandler<TimeEventArgs> Drawing;

        /// <summary>
        /// Initializes a new instance of <c>World</c>.
        /// </summary>
        public World(IServiceProvider services): this()
        {
            if (services == null)
                throw new ArgumentNullException("services");

            this.Services = services;

            var graphicsDeviceService = services.GetService<IGraphicsDeviceService>();
            if (graphicsDeviceService != null && graphicsDeviceService.GraphicsDevice != null)
            {
                this.TemplateFactories.Add(new ContentTemplateFactory(services));
                this.CreateGraphics(graphicsDeviceService.GraphicsDevice);
            }
        }

        /// <summary>
        /// Initializes a new instance of <c>World</c>.
        /// </summary>
        public World(GraphicsDevice graphics) : this()
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.Services = new GraphicsDeviceServiceProvider(graphics);
            this.TemplateFactories.Add(new ContentTemplateFactory(this.Services));
            this.CreateGraphics(graphics);
        }
        
        /// <summary>
        /// Initializes a new instance of <c>World</c>.
        /// </summary>
        public World()
        {
            Services = new EmptyServiceProvider();
            TemplateFactories = new List<ITemplateFactory>();
            TemplateFactories.Add(new ClassTemplateFactory());
            worldObjects.Added += new EventHandler<ItemChangedEventArgs<object>>(OnAdded);
            worldObjects.Removed += new EventHandler<ItemChangedEventArgs<object>>(OnRemoved);
        }

        private void OnAdded(object sender, ItemChangedEventArgs<object> e)
        {

        }


        private void OnRemoved(object sender, ItemChangedEventArgs<object> e)
        {

        }

        /// <summary>
        /// Updates all the objects managed by this world.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            if (Updating != null)
            {
                timeEventArgs.ElapsedTime = elapsedTime;
                Updating(this, timeEventArgs);
            }
        }

        /// <summary>
        /// Draws all the objects managed by this world.
        /// </summary>
        public void Draw(TimeSpan elapsedTime)
        {
            if (Drawing != null)
            {
                timeEventArgs.ElapsedTime = elapsedTime;
                Drawing(this, timeEventArgs);
            }
        }

        /// <summary>
        /// Creates an object from existing template.
        /// </summary>
        public object CreateFromTemplate(Template template)
        {
            if (template == null)
                throw new ArgumentNullException("template");

            return template.Value = TemplateFactories.First(f => f.Name == template.FactoryName)
                                                     .Create(typeof(object), template.Name);
        }

        #region Serialization
        /// <summary>
        /// Gets a collection of known assemblies.
        /// Types marked as Serializable in the known assemblies can be serialized and
        /// deserialized using <c>Save</c> and <c>FromFile</c>.
        /// </summary>
        public static ICollection<Assembly> KnownAssemblies { get { return KnownAssemblies; } }

        /// <summary>
        /// Gets a collection of known types that can be serialized and
        /// deserialized using <c>Save</c> and <c>FromFile</c>.
        /// </summary>
        public static ICollection<Type> KnownTypes { get { return knownTypes; } }

        private static List<Assembly> knownAssemblies = new List<Assembly>();
        private static List<Type> knownTypes = new List<Type>();
        private static Dictionary<Assembly, List<Type>> knownTypesDictionary = new Dictionary<Assembly, List<Type>>();

        static World()
        {
#if WINDOWS
            knownAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(
                assembly =>
                    !assembly.FullName.StartsWith("Microsoft.Xna.Framework") &&
                    !assembly.FullName.StartsWith("System") &&
                    !assembly.FullName.StartsWith("mscorlib")));
#else
            knownAssemblies.Add(Assembly.GetExecutingAssembly());
#endif
            foreach (var assembly in knownAssemblies.Distinct())
            {
                knownTypesDictionary.Add(assembly, assembly.FindSerializableTypes());
            }
        }

        /// <summary>
        /// Loads the world from file.
        /// </summary>
        public static World FromFile(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open))
            {
                return FromStream(stream);
            }
        }

        /// <summary>
        /// Loads the world from file.
        /// </summary>
        public static World FromFile(GraphicsDevice graphics, string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open))
            {
                return FromStream(graphics, stream);
            }
        }

        /// <summary>
        /// Loads the world from a stream.
        /// </summary>
        public static World FromStream(Stream stream)
        {
            try
            {
                return (World)GetSerializer().Deserialize(stream);
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Loads the world from a stream.
        /// </summary>
        public static World FromStream(GraphicsDevice graphics, Stream stream)
        {
            try
            {
                World world = (World)GetSerializer().Deserialize(stream);
                world.CreateGraphics(graphics);
                return world;
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Saves the world to a file.
        /// </summary>
        public void Save(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(stream);
            }
        }

        /// <summary>
        /// Saves the world to a stream.
        /// </summary>
        public void Save(Stream stream)
        {
            try
            {
                GetSerializer().Serialize(stream, this);
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }
        }

        private static XmlSerializer GetSerializer()
        {
            return new XmlSerializer(typeof(World),
                                    SerializationExtensions.XmlAttributeOverrides,
                                    GetExtraTypes(), null, null);
        }

        private static Type[] GetExtraTypes()
        {
            var extraTypes = knownTypes.Concat(knownAssemblies.SelectMany(assembly =>
            {
                List<Type> types;
                if (!knownTypesDictionary.TryGetValue(assembly, out types))
                    knownTypesDictionary[assembly] = types = assembly.FindSerializableTypes();
                return types;
            })).ToArray();
            return extraTypes;
        }
        #endregion
    }
}
