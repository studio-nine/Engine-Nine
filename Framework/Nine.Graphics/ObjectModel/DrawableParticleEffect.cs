#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ObjectModel
{
    public class DrawableParticleEffect : Drawable, ISpatialQueryable
    {
        [ContentSerializer(Optional = true)]
        public Vector3 Position
        {
            get { return position; }
            set { Transform = Matrix.CreateTranslation(value); }
        }

        private Vector3 position;

        protected override void OnTransformChanged()
        {
            position = Transform.Translation;
            base.OnTransformChanged();
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        [ContentSerializer(Optional = true)]
        public ParticleEffect Source { get; set; }
        
        public BoundingBox BoundingBox
        {
            get { /* TODO: */ return Source.BoundingBox; }
        }

        object ISpatialQueryable.SpatialData { get; set; }

        public event EventHandler<EventArgs> BoundingBoxChanged;

        public override void Update(TimeSpan elapsedTime)
        {
            if (Source != null)
                Source.Update(elapsedTime);
        }

        public override void Draw(GraphicsContext context)
        {
            if (Source == null || Visible)
            {
                // FIXME: We might be drawing it twice if 2 DrawableParticleEffect share the same ParticleEffect !!!
                context.ParticleBatch.Draw(Source);
            }
        }
    }
}