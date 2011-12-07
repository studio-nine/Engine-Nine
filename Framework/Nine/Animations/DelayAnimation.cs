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
    /// An dummy animation that completes after the specified duration.
    /// This is usually used with <c>SequentialAnimation</c> to delay
    /// the playing of subsequent animations.
    /// </summary>
    [ContentSerializable]
    public class DelayAnimation : Animation
    {
        /// <summary>
        /// Gets or sets the duration of this animation.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets the elapsed time since the playing of this delay animation.
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayAnimation"/> class.
        /// </summary>
        public DelayAnimation() { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DelayAnimation"/> class.
        /// </summary>
        public DelayAnimation(float seconds) { Duration = TimeSpan.FromSeconds(seconds); }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DelayAnimation"/> class.
        /// </summary>
        public DelayAnimation(TimeSpan duration) { Duration = duration; }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            ElapsedTime = TimeSpan.Zero;
            base.OnStarted();
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public override void Update(TimeSpan elapsedTime)
        {
            ElapsedTime += elapsedTime;
            if (ElapsedTime > Duration)
            {
                Stop();
                OnCompleted();
            }
        }
    }
}