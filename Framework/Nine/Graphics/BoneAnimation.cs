#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    using Nine.Animations;

    #region Keyframe
    /// <summary>
    /// Describes the position of a single bone at a single point in time.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Keyframe
    {
        /// <summary>
        /// Gets the index of the target bone that is animated by this keyframe.
        /// </summary>
        [ContentSerializer]
        public int Bone { get; private set; }


        /// <summary>
        /// Gets the time offset from the start of the animation to this keyframe.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Time { get; private set; }


        /// <summary>
        /// Gets the bone transform for this keyframe.
        /// </summary>
        [ContentSerializer]
        public Matrix Transform { get; private set; }


        /// <summary>
        /// Constructs a new keyframe object.
        /// </summary>
        public Keyframe(int bone, TimeSpan time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private Keyframe() { }
    }
    #endregion

    #region AnimationClip
    /// <summary>
    /// It holds all the keyframes needed to describe a single animation.
    /// </summary>
    public class AnimationClip
    {
        /// <summary>
        /// Gets the total length of the animation.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Duration { get; set; }


        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<Keyframe> Keyframes { get; private set; }


        /// <summary>
        /// Constructs a new animation clip object.
        /// </summary>
        public AnimationClip(TimeSpan duration, List<Keyframe> keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        }

        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private AnimationClip() { }
    }
    #endregion

    #region BoneAnimation
    /// <summary>
    /// An animation player used to play bone animations.
    /// </summary>
    public class BoneAnimation : IAnimation
    {
        /// <summary>
        /// Gets the current state of the animation.
        /// </summary>
        public AnimationState State { get; private set; }

        public float Speed { get; set; }
        public AnimationClip AnimationClip { get; set; }
        public int CurrentFrame { get; private set; }
        public TimeSpan CurrentTime { get; private set; }


        public event EventHandler Completed;


        Matrix[] boneTransforms;
        Model model;


        public BoneAnimation() 
        {
            Speed = 1.0f;
            State = AnimationState.Playing;
        }

        public BoneAnimation(Model model, AnimationClip clip)
        {
            Speed = 1.0f;
            State = AnimationState.Playing;

            Model = model;
            AnimationClip = clip;
        }


        public Model Model 
        {
            get { return model; }

            set
            {
                model = value;
                boneTransforms = new Matrix[model.Bones.Count];
                model.CopyBoneTransformsTo(boneTransforms);
            }
        }
        
        public TimeSpan Duration 
        {
            get 
            {
                return AnimationClip != null ? AnimationClip.Duration : TimeSpan.Zero; 
            }
        }
        
        public void Play()
        {
            CurrentTime = TimeSpan.Zero;
            CurrentFrame = 0;

            State = AnimationState.Playing;
        }

        public void Stop()
        {
            CurrentTime = TimeSpan.Zero;
            CurrentFrame = 0;

            State = AnimationState.Stopped;
        }

        public void Pause()
        {
            State = AnimationState.Paused;
        }

        public void Resume()
        {
            State = AnimationState.Playing;
        }


        public void Update(GameTime gameTime)
        {
            if (State == AnimationState.Playing|| model == null || AnimationClip == null)
                return;

            CurrentTime += TimeSpan.FromMilliseconds(gameTime.ElapsedGameTime.TotalMilliseconds * Speed);

            if (CurrentTime > AnimationClip.Duration)
            {
                CurrentTime = TimeSpan.Zero;
                CurrentFrame = 0;

                // Trigger complete event
                if (Completed != null)
                    Completed(this, EventArgs.Empty);
            }


            // Read keyframe matrices.
            IList<Keyframe> keyframes = AnimationClip.Keyframes;

            while (CurrentFrame < keyframes.Count)
            {
                Keyframe keyframe = keyframes[CurrentFrame];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > CurrentTime)
                    break;

                // Use this keyframe.      
                boneTransforms[keyframe.Bone] = keyframe.Transform;

                CurrentFrame++;
            }

            Model.CopyBoneTransformsFrom(boneTransforms);
        }

        public void Seek(TimeSpan time)
        {
            if (model == null || AnimationClip == null)
                return;

            CurrentTime = time;

            if (CurrentTime < TimeSpan.Zero)
                CurrentTime = TimeSpan.Zero;

            if (CurrentTime > AnimationClip.Duration)
                CurrentTime = AnimationClip.Duration;

            CurrentFrame = 0;

            IList<Keyframe> keyframes = AnimationClip.Keyframes;

            while (CurrentFrame < keyframes.Count)
            {
                Keyframe keyframe = keyframes[CurrentFrame];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > CurrentTime)
                    break;

                // Use this keyframe.
                boneTransforms[keyframe.Bone] = keyframe.Transform;

                CurrentFrame++;
            }

            Model.CopyBoneTransformsFrom(boneTransforms);
        }

        public void Seek(int frame)
        {
            if (model == null || AnimationClip == null)
                return;
            
            IList<Keyframe> keyframes = AnimationClip.Keyframes;

            CurrentFrame = frame;

            if (CurrentFrame < 0)
                CurrentFrame = 0;
            if (CurrentFrame > keyframes.Count)
                CurrentFrame = keyframes.Count;

            int counter = 0;

            while (counter < CurrentFrame)
            {
                Keyframe keyframe = keyframes[counter];

                // Stop when we've read up to the current time position.
                CurrentTime = keyframe.Time;

                // Use this keyframe.
                boneTransforms[keyframe.Bone] = keyframe.Transform;

                counter++;
            }

            Model.CopyBoneTransformsFrom(boneTransforms);
        }
    }
    #endregion
}
