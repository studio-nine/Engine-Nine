namespace Nine.Studio.Extensibility
{
    using System;

    /// <summary>
    /// Represents an interface that is used to visualize a document of the given type.
    /// </summary>
    public interface IVisualizer
    {
        /// <summary>
        /// Gets the type of object that can be visualized.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Visualizes the target object.
        /// </summary>
        object Visualize(object targetObject);
    }

    /// <summary>
    /// Generic base class implementing IVisualizer
    /// </summary>
    public abstract class Visualizer<T> : IVisualizer
    {
        /// <summary>
        /// Gets the type of object that can be visualized.
        /// </summary>
        public Type TargetType { get { return typeof(T); } }

        object IVisualizer.Visualize(object targetObject)
        {
            Verify.IsAssignableFrom(targetObject, typeof(T), "targetObject");            
            return Visualize((T)targetObject);
        }

        /// <summary>
        /// Visualizes the specified target object.
        /// </summary>
        protected abstract object Visualize(T targetObject);
    }
}
