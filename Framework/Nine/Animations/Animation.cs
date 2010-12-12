#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Animations
{
    /// <summary>
    /// Base class for all playable animations.
    /// </summary>
    public abstract class Animation : IUpdateObject, IAnimation
    {
        /// <summary>
        /// Gets the current state of the animation.
        /// </summary>
        public AnimationState State { get; private set; }

        /// <summary>
        /// Plays the animation from start position.
        /// </summary>
        public void Play()
        {
            State = AnimationState.Playing;
            OnStarted();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            State = AnimationState.Stopped;
            OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            if (State == AnimationState.Playing)
            {
                State = AnimationState.Paused;
                OnPaused();
            }
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        public void Resume()
        {
            if (State == AnimationState.Paused)
            {
                State = AnimationState.Playing;
                OnResumed();
            }
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected virtual void OnStarted()
        {
            if (Started != null)
                Started(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected virtual void OnStopped()
        {
            if (Stopped != null)
                Stopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected virtual void OnPaused()
        {
            if (Paused != null)
                Paused(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected virtual void OnResumed()
        {
            if (Resumed != null)
                Resumed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when this animation has completely finished playing.
        /// </summary>
        public abstract event EventHandler Completed;

        /// <summary>
        /// Occurs when this animation has started playing.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when this animation has stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when this animation has been paused.
        /// </summary>
        public event EventHandler Paused;

        /// <summary>
        /// Occurs when this animation has been resumed after pause.
        /// </summary>
        public event EventHandler Resumed;
    }

}