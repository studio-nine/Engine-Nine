namespace Nine.Graphics.ObjectModel
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    
    /// <summary>
    /// Represents a drawable object that can be rendered using the renderer.
    /// </summary>
    public interface IDrawableObject
    {
        /// <summary>
        /// Gets whether this object is visible.
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Gets the material of the object.
        /// A value of null indicates the object does not have any user specific
        /// material settings, and should be drawn using the default method.

        /// </summary>
        Material Material { get; }

        /// <summary>
        /// Perform any updates before this object is drawed.
        /// </summary>
        void BeginDraw(DrawingContext context);

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        void Draw(DrawingContext context, Material material);

        /// <summary>
        /// Perform any updates after this object is drawed.
        /// </summary>
        void EndDraw(DrawingContext context);
    }

    /// <summary>
    /// Defines an interface for objects that receives lights and shadows.
    /// </summary>
    public interface ILightable
    {
        /// <summary>
        /// Gets whether lighting is enabled on this drawable.
        /// </summary>
        bool LightingEnabled { get; }

        /// <summary>
        /// Gets whether the lighting system should draw multi-pass lighting
        /// overlays on to this object.
        /// </summary>
        bool MultiPassLightingEnabled { get; }

        /// <summary>
        /// Gets the max number of affecting lights.
        /// </summary>
        int MaxAffectingLights { get; }

        /// <summary>
        /// Gets whether the drawable casts shadow.
        /// </summary>
        bool CastShadow { get; }

        /// <summary>
        /// Gets whether the drawable receives shadow.
        /// </summary>
        bool ReceiveShadow { get; }

        /// <summary>
        /// Gets whether the lighting system should draw multi-pass shadow
        /// overlays on to this object.
        /// </summary>
        bool MultiPassShadowEnabled { get; }

        /// <summary>
        /// Gets the max number of received shadows.
        /// </summary>
        int MaxReceivedShadows { get; }

        /// <summary>
        /// Gets or sets the data used by the lighting and shadowing system.
        /// </summary>
        object LightingData { get; set; }
    }

    /// <summary>
    /// Defines an interface for objects that supports hardware instancing
    /// </summary>
    public interface ISupportInstancing
    {
        /// <summary>
        /// Gets the subset count.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the material for the given subset.
        /// </summary>
        Material GetMaterial(int subset);

        /// <summary>
        /// Prepares the material for rendering.
        /// </summary>
        void PrepareMaterial(int subset, Material material);

        /// <summary>
        /// Gets the vertex buffer for the given subset.
        /// </summary>
        void GetVertexBuffer(int subset, out VertexBuffer vertexBuffer, out int vertexOffset, out int numVertices);
        
        /// <summary>
        /// Gets the index buffer for the given subset.
        /// </summary>
        void GetIndexBuffer(int subset, out IndexBuffer indexBuffer, out int startIndex, out int primitiveCount);
    }
}