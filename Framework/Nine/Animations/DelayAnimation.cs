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
    public class DelayAnimation : TimelineAnimation
    {
        /// <summary>
        /// Gets or sets the duration of this animation.
        /// </summary>
        public new TimeSpan Duration { get; set; }

        public DelayAnimation() { Repeat = 1; }

        public DelayAnimation(float seconds) { Duration = TimeSpan.FromSeconds(seconds); Repeat = 1; }

        public DelayAnimation(TimeSpan duration) { Duration = duration; Repeat = 1; }

        protected override TimeSpan GetDuration()
        {
            return Duration;
        }

        protected override void OnSeek(TimeSpan position, TimeSpan previousPosition) { }
    }
}