#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics;
using Nine.Animations;
using Nine.Graphics.ParticleEffects;

namespace Nine
{
    public abstract class Magic
    {
        public ContentManager Content { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public ParticleEffect ParticleEffect { get; protected set; }

        protected Magic(ContentManager content, string asset)
        {
            this.Content = content;
            this.GraphicsDevice = content.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice;
            if (!string.IsNullOrEmpty(asset))
            {
                this.ParticleEffect = content.Load<ParticleEffect>(asset);
            }
        }

        public virtual bool Update(TimeSpan elapsedTime, KinectSkeleton skeleton)
        {
            KinectSkeletonTag tag = skeleton.Tag as KinectSkeletonTag;

            if (skeleton.Tag == null)
            {
                skeleton.Tag = tag = new KinectSkeletonTag();

                tag.LeftHand = new KinectGestureTracker(JointID.HandLeft, skeleton);
                tag.RightHand = new KinectGestureTracker(JointID.HandRight, skeleton);

                OnInitialize(skeleton, tag);
            }

            tag.LeftHand.Update(elapsedTime);
            tag.RightHand.Update(elapsedTime);

            return Update(elapsedTime, skeleton, tag);
        }

        protected abstract void OnInitialize(KinectSkeleton skeleton, KinectSkeletonTag tag);

        protected abstract bool Update(TimeSpan elapsedTime, KinectSkeleton skeleton, KinectSkeletonTag tag);

        public virtual void Draw(TimeSpan elapsedTime, ParticleBatch particleBatch)
        {
            if (ParticleEffect != null)
            {
                ParticleEffect.Update(elapsedTime);
                particleBatch.Draw(ParticleEffect);
            }
        }

        public Vector2 GetDisplayPosition2D(KinectSkeleton skeleton, JointID joint)
        {
            return skeleton.Project(joint, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        }

        public Vector3 GetDisplayPosition(KinectSkeleton skeleton, JointID joint)
        {
            return new Vector3(skeleton.Project(joint, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), 0);
        }

        public abstract void Enable(KinectSkeleton skeleton);
        public abstract void Disable(KinectSkeleton skeleton);
    }
}
