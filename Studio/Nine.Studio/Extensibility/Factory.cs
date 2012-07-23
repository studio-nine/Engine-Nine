namespace Nine.Studio.Extensibility
{
    using System;
    using System.Reflection;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Represents a factory that can create a new document.
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
        object Create(Editor editor, object parent);
    }

    /// <summary>
    /// Generic base class implementing IDocumentType
    /// </summary>
    public abstract class Factory<T, TParent> : IFactory
    {
        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }
        private static GraphicsDevice graphicsDevice;

        static Factory()
        {
            // Force graphics device service to be initialized upfront to walkaround a bug in Win8.
            graphicsDevice = Nine.Graphics.GraphicsDeviceService.AddRef().GraphicsDevice;
        }
        
        public Editor Editor { get; private set; }
        public Type TargetType { get { return typeof(T); } }

        public virtual object Create(TParent parent)
        {
            var type = typeof(T);
            var graphicsDeviceConstructor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(GraphicsDevice) }, null);
            if (graphicsDeviceConstructor != null)
                return graphicsDeviceConstructor.Invoke(new object[] { GraphicsDevice });
            return Activator.CreateInstance<T>();
        }

        object IFactory.Create(Editor editor, object parent)
        {
            Verify.IsNotNull(editor, "editor");
            Verify.IsAssignableFrom(parent, typeof(TParent), "parent");

            Editor = editor;
            var createdObject = Create((TParent)parent);

            Verify.IsNotNull(createdObject, "createdObject");
            return createdObject;
        }
    }
}
