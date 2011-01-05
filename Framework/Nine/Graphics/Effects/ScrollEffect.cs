#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    public partial class ScrollEffect : IEffectMatrices, IEffectTexture
    {
        public float Speed { get; set; }
        public float Direction { get; set; }
        public bool TextureEnabled { get; set; }

        private TimeSpan startTime = TimeSpan.Zero;
                        
        private void OnCreated() 
        {
            Speed = 1.0f;
            TextureScale = Vector2.One;
        }

        private void OnClone(ScrollEffect cloneSource) 
        {
            Speed = cloneSource.Speed;
            Direction = cloneSource.Direction;
            TextureEnabled = cloneSource.TextureEnabled;
        }

        private void OnApplyChanges() 
        {

        }

        public void Update(GameTime time)
        {
            if (startTime == TimeSpan.Zero)
                startTime = time.TotalGameTime;

            TimeSpan duration = time.TotalGameTime - startTime;

            float dx = (float)(duration.TotalSeconds * Speed * Math.Cos(Direction));
            float dy = (float)(duration.TotalSeconds * Speed * Math.Sin(Direction));


            textureOffset = new Vector2(dx, dy);            
        }

        void IEffectTexture.SetTexture(string name, Texture texture) { }
    }

#endif
}