namespace Nine.Graphics
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Defines an interface for objects that can be added to or removed 
    /// from a drawing context.
    /// </summary>
    public interface ISceneObject
    {
        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        void OnAdded(DrawingContext context);

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void OnRemoved(DrawingContext context);
    }

    /// <summary>
    /// Interface for game cameras.
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Gets the view, projection matrix of this camera.
        /// </summary>
        /// <returns>
        /// Returns a value indicating whether the content this camera will be
        /// rendered onto the screen.
        /// </returns>
        bool TryGetViewFrustum(out Matrix view, out Matrix projection);
    }

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
        /// Gets whether the drawable casts shadow.
        /// </summary>
        bool CastShadow { get; }

        /// <summary>
        /// Gets the material of the object.
        /// A value of null indicates the object does not have any user specific
        /// material settings, and should be drawn using the default method.
        /// </summary>
        Material Material { get; }

        /// <summary>
        /// Gets the distance from the position of the object to the current camera.
        /// </summary>
        float GetDistanceToCamera(Vector3 cameraPosition);

        /// <summary>
        /// Called every frame when this object is added to the main view frustum.
        /// </summary>
        void OnAddedToView(DrawingContext context);

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        void Draw(DrawingContext context, Material material);
    }

    /// <summary>
    /// Defines an 2D drawable object
    /// </summary>
    public interface ISprite
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
        /// Gets the z order of this sprite.
        /// </summary>
        int ZOrder { get; }

        /// <summary>
        /// Gets the blend state of this sprite.
        /// A return value of null indicates that this sprite will use BlendState.AlphaBlend.
        /// </summary>
        BlendState BlendState { get; }

        /// <summary>
        /// Gets the sampler state of this sprite.
        /// A return value of null indicates that this sprite will use SamplerState.LinearClamp.
        /// </summary>
        SamplerState SamplerState { get; }

        /// <summary>
        /// Draws this sprite using sprite batch.
        /// </summary>
        void Draw(DrawingContext context, SpriteBatch spriteBatch);

        /// <summary>
        /// Draws this sprite using the specified material.
        /// </summary>
        void Draw(DrawingContext context, Material material);
    }

    /// <summary>
    /// Defines an interface for objects that receives lights and shadows.
    /// </summary>
    interface ILightable
    {
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
    /// Defines an interface for post processing effect
    /// </summary>
    public interface IPostEffect
    {
        /// <summary>
        /// Gets or sets the input texture to be processed.
        /// </summary>
        Texture2D InputTexture { get; set; }

        /// <summary>
        /// Gets the preferred surface format for the input texture.
        /// </summary>
        SurfaceFormat? InputFormat { get; }
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
        /// Gets the bounding box of this instance.
        /// </summary>
        BoundingBox BoundingBox { get; }

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

    /// <summary>
    /// Defines an object that supports debug visual.
    /// </summary>
    public interface IDebugDrawable
    {
        /// <summary>
        /// Gets whether this object is visible.
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Draws the debug overlay of this object.
        /// </summary>
        void Draw(DrawingContext context, DynamicPrimitive primitive);
    }
}