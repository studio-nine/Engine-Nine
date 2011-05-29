#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace Nine.Animations
{
#if XBOX

    public class AvatarAnimation : IAvatarAnimation
    {
        public ReadOnlyCollection<Matrix> BoneTransforms
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The current temporal position in the animation.
        /// </summary>
        private TimeSpan currentPosition = TimeSpan.Zero;

        /// <summary>
        /// The current temporal position in the animation.
        /// </summary>
        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set
            {
                currentPosition = value;
                Update(TimeSpan.Zero, false);
            }
        }

        public AvatarExpression Expression
        {
            get { throw new NotImplementedException(); }
        }

        public TimeSpan Length
        {
            get { throw new NotImplementedException(); }
        }

        public void Update(TimeSpan elapsedAnimationTime, bool loop)
        {

        }
    }

#endif
}
