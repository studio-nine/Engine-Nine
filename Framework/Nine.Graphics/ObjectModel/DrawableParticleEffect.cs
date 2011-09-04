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
    public class DrawableParticleEffect : Drawable
    {
        #region Position
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
        }
        #endregion

        [ContentSerializer(Optional = true)]
        public ParticleEffect ParticleEffect { get; set; }
        
        public override BoundingBox BoundingBox
        {
            get { /* TODO: */ return ParticleEffect.BoundingBox; }
        }

        public override void Update(TimeSpan elapsedTime)
        {
            if (ParticleEffect != null)
                ParticleEffect.Update(elapsedTime);
        }

        public override void Draw(GraphicsContext context)
        {
            if (ParticleEffect == null || Visible)
            {
                context.ParticleBatch.Draw(ParticleEffect);
            }
        }
    }
}