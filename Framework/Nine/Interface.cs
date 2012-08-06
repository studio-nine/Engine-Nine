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
    /// Interface for an object with a bounding box that can be queryed.
    /// </summary>
    public interface IBoundable
    {
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        BoundingBox BoundingBox { get; }
    }

    /// <summary>
    /// Interface for a 3D geometry made up of triangles
    /// </summary>
    public interface IGeometry
    {
        //void GetTriangles(int triangleUsage, ref BoundingBox bounds, out Vector3[] vertices, out ushort[] indices, out int[][] adjacencies);

        /// <summary>
        /// Gets the optional transformation matrix of this <see cref="IGeometry"/>.
        /// </summary>
        Matrix? Transform { get; }

        /// <summary>
        /// Gets a readonly list of vertex positions.
        /// </summary>
        Vector3[] Positions { get; }

        /// <summary>
        /// Gets a read-only list of geometry indices.
        /// </summary>
        ushort[] Indices { get; }
    }

    /// <summary>
    /// Defines Spatial relations between objects.
    /// </summary>
    public interface ISpatialQuery
    {
        /// <summary>
        /// Creates a spatial query of the specified target type.
        /// </summary>
        ISpatialQuery<T> CreateSpatialQuery<T>() where T : class;
    }

    /// <summary>
    /// Defines Spatial relations between objects.
    /// </summary>
    public interface ISpatialQuery<T>
    {
        /// <summary>
        /// Finds all the objects that intersects with the specified ray.
        /// </summary>
        /// <param name="result">The caller is responsable for clearing the result collection</param>
        void FindAll(ref Ray ray, ICollection<T> result);

        /// <summary>
        /// Finds all the objects resides within the specified bounding sphere.
        /// </summary>
        /// <param name="result">The caller is responsable for clearing the result collection</param>
        void FindAll(ref BoundingSphere boundingSphere, ICollection<T> result);

        /// <summary>
        /// Finds all the objects that intersects with the specified bounding box.
        /// </summary>
        /// <param name="result">The caller is responsable for clearing the result collection</param>
        void FindAll(ref BoundingBox boundingBox, ICollection<T> result);

        /// <summary>
        /// Finds all the objects resides within the specified bounding frustum.
        /// </summary>
        /// <param name="result">The caller is responsable for clearing the result collection</param>
        void FindAll(BoundingFrustum boundingFrustum, ICollection<T> result);
    }

    /// <summary>
    /// Interface for an object that can be queried by a scene manager.
    /// </summary>
    public interface ISpatialQueryable : IBoundable
    {
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
    /// Defines a factory that can create an instance of a object based on the
    /// specified type name.
    /// </summary>
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates a new instance of the object with the specified type name.
        /// </summary>
        T Create<T>(string typeName);
    }

#if !WINDOWS
    /// <summary>
    /// Supports cloning, which creates a new instance of a class with the same value
    /// as an existing instance.
    /// </summary>
    public interface ICloneable
    {
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        object Clone();
    }
#endif
}