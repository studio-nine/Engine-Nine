#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Graphics
{
    public class SpriteAnimation : IAnimation
    {
        public float Speed { get; set; }
        public Texture2D CurrentFrame { get; set; }
        public List<Texture2D> Frames { get; set; }
        
        private TimeSpan startTime = TimeSpan.Zero;


        public SpriteAnimation()
        {
            Speed = 1.0f;
            Frames = new List<Texture2D>();
        }

        public SpriteAnimation(IEnumerable<Texture2D> textures)
        {
            Speed = 1.0f;
            Frames = new List<Texture2D>(textures);
        }

        public void Update(GameTime time)
        {
            if (startTime == TimeSpan.Zero)
                startTime = time.TotalGameTime;

            TimeSpan duration = time.TotalGameTime - startTime;

            CurrentFrame = Frames[(int)(duration.TotalMilliseconds * 0.001f * Speed * 24) % Frames.Count];
        }

        #region IAnimation Members


        public TimeSpan Duration
        {
            get { throw new NotImplementedException(); }
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public event EventHandler Complete;

        #endregion
    }
}
