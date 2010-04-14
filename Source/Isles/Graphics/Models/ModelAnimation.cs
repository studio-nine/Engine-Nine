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
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics.Models
{
    public class ModelAnimation : IAnimation
    {
        public float Speed { get; set; }
        public bool IsPlaying { get; private set; }
        public AnimationClip AnimationClip { get; set; }
        public int CurrentFrame { get; private set; }
        public TimeSpan CurrentTime { get; private set; }


        public event EventHandler Complete;


        Matrix[] boneTransforms;
        Model model;


        public ModelAnimation() 
        {
            Speed = 1.0f;
            IsPlaying = true;
        }

        public ModelAnimation(Model model, AnimationClip clip)
        {
            Speed = 1.0f;
            IsPlaying = true;

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

            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }

        public void Resume()
        {
            IsPlaying = true;
        }


        public void Update(GameTime gameTime)
        {
            if (!IsPlaying || model == null || AnimationClip == null)
                return;

            CurrentTime += TimeSpan.FromMilliseconds(gameTime.ElapsedGameTime.TotalMilliseconds * Speed);

            if (CurrentTime > AnimationClip.Duration)
            {
                CurrentTime = TimeSpan.Zero;
                CurrentFrame = 0;

                // Trigger complete event
                if (Complete != null)
                    Complete(this, EventArgs.Empty);
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
}
