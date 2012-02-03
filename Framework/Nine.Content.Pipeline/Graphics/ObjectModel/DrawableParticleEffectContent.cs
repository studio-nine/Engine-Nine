#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nine.Content.Pipeline.Graphics.ParticleEffects;
#endregion

namespace Nine.Content.Pipeline.Graphics.ObjectModel
{
    [ContentProperty("Content")]
    partial class DrawableParticleEffectContent
    {
        [ContentSerializer(Optional=true)]
        public virtual ParticleEffectContent Content { get; set; }
        
        [ContentSerializer(Optional = true)]
        public virtual Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateTransform(); }
        }
        Vector3 position;

        [ContentSerializer(Optional = true)]
        public virtual Vector3 Direction
        {
            get { return direction; }
            set { direction = value; UpdateTransform(); }
        }
        Vector3 direction = Vector3.UnitZ;

        private void UpdateTransform()
        {
            Transform = MatrixHelper.CreateWorld(Position, Direction);
        }

        [SelfProcess]
        static DrawableParticleEffectContent Process(DrawableParticleEffectContent input, ContentProcessorContext context)
        {
            if (input.Content != null)
            {
                if (input.ParticleEffect != null && !string.IsNullOrEmpty(input.ParticleEffect.Filename))
                {
                    throw new InvalidContentException("You cannot set Content and ParticleEffect property at the same time.");
                }

                input.ParticleEffect = context.BuildAsset<object, object>(input.Content, "DefaultContentProcessor").Filename;
            }
            else if (input.ParticleEffect != null && !string.IsNullOrEmpty(input.ParticleEffect.Filename))
            {
                context.Logger.LogWarning(null, null, "Particle Effect not specified.");
            }

            return input;
        }
    }
}
