namespace Nine
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    
    /// <summary>
    /// Object that react to game updates.
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        void Update(TimeSpan elapsedTime);
    }

    /// <summary>
    /// Object that react to game draws.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Draws the internal state of the object.
        /// </summary>
        void Draw(TimeSpan elapsedTime);
    }

    /// <summary>
    /// Interface for a 3D geometry made up of triangles
    /// </summary>
    public interface IGeometry
    {
        /// <summary>
        /// Gets an optional world transform matrix of the target geometry.
        /// </summary>
        Matrix Transform { get; }

        /// <summary>
        /// Gets the triangle vertices of the target geometry.
        /// </summary>
        /// <param name="vertices">Output vertex buffer</param>
        /// <param name="indices">Output index buffer</param>
        /// <returns>
        /// Returns whether the result contains any triangles.
        /// </returns>
        bool TryGetTriangles(out Vector3[] vertices, out ushort[] indices);
    }

    /// <summary>
    /// Defines Spatial relations between objects.
    /// </summary>
    public interface ISpatialQuery
    {
        /// <summary>
        /// Creates a spatial query of the specified target type.
        /// </summary>
        ISpatialQuery<T> CreateSpatialQuery<T>(Predicate<T> condition) where T : class;
    }

    /// <summary>
    /// Defines Spatial relations between objects.
    /// </summary>
    public interface ISpatialQuery<T>
    {
        /// <summary>
        /// Finds all the objects that intersects with the specified ray.
        /// </summary>
        /// <param name="result">The caller is responsible for clearing the result collection</param>
        void FindAll(ref Ray ray, ICollection<T> result);

        /// <summary>
        /// Finds all the objects resides within the specified bounding sphere.
        /// </summary>
        /// <param name="result">The caller is responsible for clearing the result collection</param>
        void FindAll(ref BoundingSphere boundingSphere, ICollection<T> result);

        /// <summary>
        /// Finds all the objects that intersects with the specified bounding box.
        /// </summary>
        /// <param name="result">The caller is responsible for clearing the result collection</param>
        void FindAll(ref BoundingBox boundingBox, ICollection<T> result);

        /// <summary>
        /// Finds all the objects resides within the specified bounding frustum.
        /// </summary>
        /// <param name="result">The caller is responsible for clearing the result collection</param>
        void FindAll(BoundingFrustum boundingFrustum, ICollection<T> result);
    }

    /// <summary>
    /// Interface for an object that can be queried by a scene manager.
    /// </summary>
    public interface ISpatialQueryable
    {
        /// <summary>
        /// Gets the axis aligned bounding box of this instance in world space.
        /// </summary>
        BoundingBox BoundingBox { get; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        event EventHandler<EventArgs> BoundingBoxChanged;

        /// <summary>
        /// Gets or sets the data used for spatial query.
        /// </summary>
        object SpatialData { get; set; }
    }

    /// <summary>
    /// Interface for a scene manager that manages the spatial relationships
    /// between objects.
    /// </summary>
    public interface ISceneManager<T> : ICollection<T>, ISpatialQuery<T>
    {

    }

    /// <summary>
    /// This interface supports the infrastructure of the framework and is not 
    /// intended to be used by externals.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISupportTarget
    {
        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        object Target { get; set; }

        /// <summary>
        /// Gets or sets the target property.
        /// </summary>
        string TargetProperty { get; set; }
    }

    /// <summary>
    ///  Notifies clients that the collection has changed.
    /// </summary>
    public interface INotifyCollectionChanged<T>
    {
        /// <summary>
        /// Raised when a new element is added to the collection.
        /// </summary>
        event Action<T> Added;

        /// <summary>
        /// Raised when an element is removed from the collection.
        /// </summary>
        event Action<T> Removed;
    }
    
    /// <summary>
    /// Interface for an object that can be picked by a given ray.
    /// </summary>
    public interface IPickable
    {
        /// <summary>
        /// Gets whether the object contains the given point.
        /// </summary>
        bool Contains(Vector3 point);

        /// <summary>
        /// Gets the nearest intersection point from the specified picking ray.
        /// </summary>
        /// <returns>Distance to the start of the ray.</returns>
        float? Intersects(Ray ray);
    }

    /// <summary>
    /// Interface for creating new types of objects.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates a new instance of the object using the specified service provider.
        /// </summary>
        object CreateInstance(IServiceProvider serviceProvider);
    }
}