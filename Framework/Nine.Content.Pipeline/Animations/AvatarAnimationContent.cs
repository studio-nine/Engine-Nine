#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace Nine.Content.Pipeline.Animations
{
    #region AvatarExpressionKeyFrame
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public struct AvatarExpressionKeyFrame
    {
        /// <summary>
        /// The time offset from the start of the animation to this keyframe.
        /// </summary>
        public TimeSpan Time;

        /// <summary>
        /// The AvatarExpression to use at this keyframe
        /// </summary>
        public AvatarExpression Expression;

        #region Initialization

        /// <summary>
        /// Constructs a new AvatarExpressionKeyFrame object.
        /// </summary>
        public AvatarExpressionKeyFrame(TimeSpan time, AvatarExpression expression)
        {
            Time = time;
            Expression = expression;
        }

        #endregion
    }
    #endregion

    #region AvatarKeyFrame
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    public struct AvatarKeyFrame
    {
        /// <summary>
        /// The index of the target bone that is animated by this keyframe.
        /// </summary>
        public int Bone;

        /// <summary>
        /// The time offset from the start of the animation to this keyframe.
        /// </summary>
        public TimeSpan Time;

        /// <summary>
        /// The bone transform for this keyframe.
        /// </summary>
        public Matrix Transform;

        #region Initialization

        /// <summary>
        /// Constructs a new AvatarKeyFrame object.
        /// </summary>
        public AvatarKeyFrame(int bone, TimeSpan time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
        }

        #endregion
    }
    #endregion

    #region AvatarAnimationData
    /// <summary>
    /// The type contains the animation data for a single animation
    /// </summary>
    public class AvatarAnimationData
    {
        /// <summary>
        /// The name of the animation clip
        /// </summary>
        [ContentSerializer]
        public string Name { get; private set; }

        /// <summary>
        /// The total length of the animation.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Length { get; private set; }

        /// <summary>
        /// A combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<AvatarKeyFrame> Keyframes { get; private set; }

        /// <summary>
        /// A combined list containing all the keyframes for expressions,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<AvatarExpressionKeyFrame> ExpressionKeyframes { get; private set; }

        #region Initialization

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private AvatarAnimationData() { }


        /// <summary>
        /// Constructs a new CustomAvatarAnimationData object.
        /// </summary>
        public AvatarAnimationData(string name, TimeSpan length,
                                         List<AvatarKeyFrame> keyframes,
                                         List<AvatarExpressionKeyFrame> expressionKeyframes)
        {
            // safety-check the parameters
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (length <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("length",
                                          "The length of the animation cannot be zero.");
            }
            if ((keyframes == null) || (keyframes.Count <= 0))
            {
                throw new ArgumentNullException("keyframes");
            }

            // assign the parameters
            Name = name;
            Length = length;
            Keyframes = keyframes;
            ExpressionKeyframes = expressionKeyframes;
        }

        #endregion
    }
    #endregion
}
