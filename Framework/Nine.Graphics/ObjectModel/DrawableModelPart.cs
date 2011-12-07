#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Animations;
using Nine.Graphics.Effects;
#if WINDOWS || XBOX
using Nine.Graphics.Effects.Deferred;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines a part of a model that contains only one material.
    /// </summary>
    [ContentSerializable]
    public class DrawableModelPart : IDrawableObject, ILightable, IContainedObject
    {
        #region Properties
        /// <summary>
        /// Gets the containing model.
        /// </summary>
        public DrawableModel Model { get; private set; }

        /// <summary>
        /// Gets the model mesh part.
        /// </summary>
        internal Microsoft.Xna.Framework.Graphics.ModelMesh ModelMesh;

        /// <summary>
        /// Gets the model mesh part.
        /// </summary>
        internal Microsoft.Xna.Framework.Graphics.ModelMeshPart ModelMeshPart;

        /// <summary>
        /// Gets or sets the visibility of this model part.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Visibility can be controlled by both ModelPart and Model.
        /// </summary>
        bool IDrawableObject.Visible { get { return Visible && Model.Visible; } }

        /// <summary>
        /// Gets or sets the material of this model part.
        /// </summary>
        public Material Material
        {
            get 
            {
                if (Model != null)
                    Model.UpdateMaterials();
                return material;
            }
            set { material = value; }
        }
        internal Material material;
        
        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        
        IContainer IContainedObject.Parent
        {
            get { return Model; }
        }
        #endregion

        #region ILightable
        bool ILightable.CastShadow { get { return Model.CastShadow; } }
        bool ILightable.ReceiveShadow { get { return Model.ReceiveShadow; } }
        bool ILightable.LightingEnabled { get { return Model.LightingEnabled; } }

        int ILightable.MaxReceivedShadows { get { return Model.MaxReceivedShadows; } }
        int ILightable.MaxAffectingLights { get { return Model.MaxAffectingLights; } }

        bool ILightable.MultiPassLightingEnabled { get { return Model.MultiPassLightingEnabled; } }
        bool ILightable.MultiPassShadowEnabled { get { return Model.MultiPassShadowEnabled; } }

        object ILightable.LightingData { get; set; }
        #endregion

        /// <summary>
        /// For content serializer.
        /// </summary>
        internal DrawableModelPart() 
        {
            this.Visible = true;
        }
        
        /// <summary>
        /// ModelPart should only be created by Model.
        /// </summary>
        internal DrawableModelPart(DrawableModel model, ModelMesh mesh, ModelMeshPart part, Material material)
        {
            if (model == null || mesh == null || part == null)
                throw new ArgumentNullException();

            this.Visible = true;
            this.Model = model;
            this.ModelMesh = mesh;
            this.ModelMeshPart = part;
            this.Material = material;
        }

        /// <summary>
        /// Draws the object using the graphics context.
        /// </summary>
        /// <param name="context"></param>
        public void Draw(GraphicsContext context) 
        {
            Model.DrawPart(context, this);
        }

        /// <summary>
        /// Draws the object with the specified effect.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="effect"></param>
        public void Draw(GraphicsContext context, Effect effect)
        {
            Model.DrawPart(context, this, effect);
        }

        void IDrawableObject.BeginDraw(GraphicsContext context) { }
        void IDrawableObject.EndDraw(GraphicsContext context) { }
    }
}