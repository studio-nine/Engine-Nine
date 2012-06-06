#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines an interface for scene objects that 
    /// </summary>
    public interface ISceneObject
    {
        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        void OnAdded(DrawingContext context);
        //void OnAddedToView(DrawingContext context);

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void OnRemoved(DrawingContext context);
        //void OnRemovedFromView(DrawingContext context);
    }
}