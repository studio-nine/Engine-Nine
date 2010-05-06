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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics
{
    #region FrameAnimation
    public abstract class FrameAnimation : IAnimation
    {
        private int currentFrame = 0;
        private float timer = 0;

        public abstract int Count { get; }

        public float Speed { get; set; }

        public float FramesPerSecond { get; set; }

        public bool IsPlaying { get; private set; }

        public int CurrentFrame
        {
            get { return currentFrame; }

            private set 
            {
                int previousFrame = currentFrame;

                currentFrame = value;

                OnFrameChanged(previousFrame);

                if (FrameChanged != null)
                    FrameChanged(this, EventArgs.Empty);
            }
        }

        public TimeSpan CurrentTime
        {
            get { return TimeSpan.FromSeconds(CurrentFrame / FramesPerSecond + timer); }
        }

        public TimeSpan Duration
        {
            get { return TimeSpan.FromSeconds(Count / FramesPerSecond); }
        }

        public FrameAnimation()
        {
            Speed = 1.0f;
            IsPlaying = true;
            FramesPerSecond = 30.0f;
        }

        public void Update(GameTime time)
        {
            if (IsPlaying && Count > 0)
            {
                timer += (float)time.ElapsedGameTime.TotalSeconds * Speed;

                float targetTime = 1.0f / FramesPerSecond;

                while (timer >= targetTime)
                {
                    int frame = CurrentFrame;

                    frame = (frame + 1) % Count;

                    if (frame == 0 && Complete != null)
                    {
                        Complete(this, EventArgs.Empty);

                        // In case we do something in the Complete event
                        if (!IsPlaying)
                            return;
                    }

                    CurrentFrame = frame;

                    timer -= targetTime;
                }
            }
        }

        protected virtual void OnFrameChanged(int previousFrame) { }

        public void Play()
        {
            IsPlaying = true;
            CurrentFrame = 0;
        }

        public void Stop()
        {
            IsPlaying = false;
            CurrentFrame = 0;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Resume()
        {
            IsPlaying = true;
        }

        public void Seek(int frame)
        {
            if (frame < 0 || frame >= Count)
                throw new ArgumentOutOfRangeException();

            timer = 0;
            CurrentFrame = frame;
        }

        public void Seek(TimeSpan time)
        {
            int frame = (int)(time.TotalSeconds * FramesPerSecond) % Count;

            float targetTime = 1.0f / FramesPerSecond;
            
            timer = (float)time.TotalSeconds;

            while (timer >= targetTime)
            {
                timer -= targetTime;
            }

            CurrentFrame = frame;
        }

        public event EventHandler Complete;

        public event EventHandler FrameChanged;
    }
    #endregion

    #region SpriteAnimation
    public class SpriteAnimation : FrameAnimation
    {
        public override int Count
        {
            get { return ImageList.Count; }
        }

        public ImageList ImageList { get; private set; }
        public Texture2D Texture { get { return ImageList[CurrentFrame].Texture; } }
        public Rectangle SourceRectangle { get { return ImageList[CurrentFrame].SourceRectangle; } }

        public SpriteAnimation()
        {
            ImageList = new ImageList();
        }

        public SpriteAnimation(IEnumerable<Texture2D> textures)
        {
            ImageList = new ImageList();

            foreach (Texture2D texture in textures)
            {
                ImageList.Add(texture, texture.Bounds);
            }
        }

        public SpriteAnimation(ImageList imageList)
        {
            if (imageList == null)
                throw new ArgumentNullException();

            ImageList = imageList;
        }
    }
    #endregion
}
