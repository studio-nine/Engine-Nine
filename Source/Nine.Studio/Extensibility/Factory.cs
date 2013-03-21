namespace Nine.Studio.Extensibility
{
    using System;
    using System.Reflection;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Represents a factory that can create a new project item.
    /// </summary>
    public interface IFactory
    {
        /// <summary>
        /// Gets the type of object that can be created.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Creates a new object of this document type.
        /// </summary>
        object Create(Project project, object container);
    }

    /// <summary>
    /// Generic base class implementing IFactory
    /// </summary>
    public abstract class Factory<T, TContainer> : IFactory
    {
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }
        private static GraphicsDevice graphicsDevice;

        static Factory()
        {
            // Force graphics device service to be initialized upfront to walk around a bug in Win8.
            graphicsDevice = Nine.Graphics.GraphicsDeviceService.AddRef().GraphicsDevice;
        }

        public Editor Editor { get; private set; }
        public Project Project { get; private set; }
        public Type TargetType { get { return typeof(T); } }

        public virtual object Create(TContainer container)
        {
            var type = typeof(T);
            var graphicsDeviceConstructor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(GraphicsDevice) }, null);
            if (graphicsDeviceConstructor != null)
                return graphicsDeviceConstructor.Invoke(new object[] { GraphicsDevice });
            return Activator.CreateInstance<T>();
        }

        object IFactory.Create(Project project, object container)
        {
            Verify.IsNotNull(project, "project");
            if (container != null)
                Verify.IsAssignableFrom(container, typeof(TContainer), "container");

            Project = project;
            Editor = project.Editor;
            var createdObject = Create((TContainer)container);

            Verify.IsNotNull(createdObject, "createdObject");
            return createdObject;
        }
    }
}
