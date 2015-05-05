namespace Nine.Animations
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Nine.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Defines a bone animation clip.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BoneAnimationClip
    {
        /// <summary>
        /// Gets animation frame rate.
        /// </summary>
        [Nine.Serialization.BinarySerializable]
        public int FramesPerSecond { get; internal set; }

        /// <summary>
        /// Gets total number of frames.
        /// </summary>
        [Nine.Serialization.BinarySerializable]
        public int TotalFrames { get; internal set; }

        /// <summary>
        /// Gets the preferred ending style.
        /// </summary>
        [Nine.Serialization.BinarySerializable]
        public KeyframeEnding PreferredEnding { get; internal set; }

        /// <summary>
        /// Gets all the channels in this animation clip.
        /// The transform is ordered by bone index then ordered by frame number.
        /// </summary>
        [Nine.Serialization.BinarySerializable]
        public Matrix[][] Transforms { get; internal set; }
    }
}
