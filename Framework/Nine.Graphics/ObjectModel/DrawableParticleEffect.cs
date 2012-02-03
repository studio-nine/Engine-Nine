#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines an instance of particle effect.
    /// </summary>
    public class DrawableParticleEffect : Transformable, ISpatialQueryable, IDrawableObject
    {
        #region ParticleEffect
        /// <summary>
        /// Gets the underlying particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public ParticleEffect ParticleEffect
        {
            get { return particleEffect; }

            // For serialization
            internal set
            {
                particleEffect = value;
                particleEmitter = particleEffect.Trigger();

                // TODO: Include child effects, ending effect and controllers that might affect the bounds.
                emitterBounds = particleEmitter.BoundingBox;
                boundingBox = emitterBounds;
            }
        }

        private ParticleEffect particleEffect;

        /// <summary>
        /// Gets the current particle emitter.
        /// </summary>
        public IParticleEmitter ParticleEmitter
        {
            get { return particleEmitter; }
        }

        private IParticleEmitter particleEmitter;
        #endregion

        #region Position
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        [ContentSerializerIgnore]
        public Vector3 Position
        {
            get { return Transform.Translation; }
            set { Transform = Matrix.CreateTranslation(value); }
        }

        /// <summary>
        /// Gets the absolute position.
        /// </summary>
        public Vector3 AbsolutePosition
        {
            get { return AbsoluteTransform.Translation; }
        }

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            particleEmitter.Position = AbsoluteTransform.Translation;
            particleEmitter.Direction = AbsoluteTransform.Forward;

            boundingBox.Min = emitterBounds.Min + AbsolutePosition;
            boundingBox.Max = emitterBounds.Max + AbsolutePosition;

            base.OnTransformChanged();
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }
        #endregion

        #region Visible
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DrawableParticleEffect"/> is visible.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; if (particleEmitter != null) particleEmitter.Enabled = value; }
        }

        bool visible = true;
        #endregion

        #region BoundingBox
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public BoundingBox BoundingBox { get { return boundingBox; } }

        private BoundingBox boundingBox;
        private BoundingBox emitterBounds;

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableParticleEffect"/> class.
        /// </summary>
        internal DrawableParticleEffect() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableParticleEffect"/> class.
        /// </summary>
        /// <param name="particleEffect">The particle effect.</param>
        public DrawableParticleEffect(ParticleEffect particleEffect) : this()
        {
            if (particleEffect == null)
                throw new ArgumentNullException("particleEffect");

            this.ParticleEffect = particleEffect;
        }
        #endregion

        #region Draw
        Material IDrawableObject.Material
        {
            get { return null; }
        }

        void IDrawableObject.BeginDraw(GraphicsContext context)
        {
            particleEmitter.Enabled = false;
        }

        void IDrawableObject.Draw(GraphicsContext context)
        {
            particleEmitter.Enabled = visible;
        }

        void IDrawableObject.Draw(GraphicsContext context, Effect effect)
        {
            particleEmitter.Enabled = false;
        }

        void IDrawableObject.EndDraw(GraphicsContext context) { }
        #endregion
    }
}