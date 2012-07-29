namespace Nine.Graphics
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Defines an interface for scene objects that 
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
    /// Interface for game camera
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Gets the optional viewport of this cameara.
        /// </summary>
        Viewport? Viewport { get; }

        /// <summary>
        /// Gets the camera view matrix
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// Gets the camera projection matrix
        /// </summary>
        Matrix Projection { get; }
    }
}