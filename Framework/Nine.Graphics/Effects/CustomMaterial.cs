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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects
{
    public sealed class CustomMaterial : Material
    {
        private CustomEffect effect;

        /// <summary>
        /// Gets or sets whether this material is transparent.
        /// </summary>
        public new bool IsTransparent { get; set; }

        /// <summary>
        /// When overriden, returns whether the rendered object is transparent under
        /// current material settings.
        /// </summary>
        protected override bool IsTransparentValue
        {
            get { return IsTransparent; }
        }

        [ContentSerializer(ElementName = "Effect")]
        internal Effect EffectSerializer
        {
            get { return effect; }
            set
            {
                effect = value as CustomEffect;
                if (effect == null)
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the parameters unique to this custom material instance.
        /// </summary>
        [ContentSerializerIgnore]
        public IDictionary<string, object> Parameters { get { return ParametersSerializer; } }

        [ContentSerializer(ElementName = "Parameters")]
        internal Dictionary<string, object> ParametersSerializer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMaterial"/> class for serialization.
        /// </summary>
        internal CustomMaterial() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomMaterial"/> class.
        /// </summary>
        /// <param name="effect">The effect.</param>
        public CustomMaterial(CustomEffect effect)
        {
            if (effect == null)
                throw new ArgumentNullException("effect");
            
            this.effect = effect;
        }

        public override Effect Effect
        {
            get { return effect; }
        }

        public override void Apply()
        {
            if (Parameters != null)
            {
                foreach (var pair in Parameters)
                {
                    var param = effect.Parameters[pair.Key];
                    if (param != null)
                        param.SetValue(pair.Value);
                }
            }
        }

        public override Material Clone()
        {
            return new CustomMaterial(effect)
            {
                IsTransparent = IsTransparent,
                DepthAlphaEnabled = DepthAlphaEnabled,
            };
        }
    }
}