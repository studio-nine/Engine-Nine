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
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using Nine.Graphics;

namespace Nine
{
    public class KinectGestureTracker : IUpdateable
    {
        const int SmoothedFrames = 10;
        const float SwipeSpeedThreshold = 0.5f;
        const float MaxSwipeInterval = 0.6f;

        JointID joint;
        KinectSkeleton skeleton;
        Queue<Vector3> queuedVelocity = new Queue<Vector3>();
        Queue<long> queuedTimeStamps = new Queue<long>();
        TimeSpan lastSwipe;

        public KinectGestureTracker(JointID joint, KinectSkeleton skeleton)
        {
            this.joint = joint;
            this.skeleton = skeleton;
        }

        public long TimeStamp { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }

        public bool IsSwiping { get; private set; }

        public void Update(TimeSpan elapsedTime)
        {
            if (!skeleton.IsTracked)
                return;

            if (queuedVelocity.Count > SmoothedFrames)
            {
                queuedVelocity.Dequeue();
                queuedTimeStamps.Dequeue();
            }

            Vector3 newPosition = skeleton.GetBoneTransform(joint).Translation;
            long newTimeStamp = skeleton.SkeletonFrame.TimeStamp;

            queuedTimeStamps.Enqueue(skeleton.SkeletonFrame.TimeStamp);
            long elapsedTimeStamp = newTimeStamp - TimeStamp;
            if (elapsedTimeStamp > 0)
                queuedVelocity.Enqueue((newPosition - Position) * 1000 / (float)elapsedTimeStamp);
            else
                queuedVelocity.Enqueue(Vector3.Zero);

            Position = newPosition;
            TimeStamp = newTimeStamp;

            Vector3 velocity = new Vector3();

            velocity.X = queuedVelocity.Average(v => Math.Min(v.X, 2));
            velocity.Y = queuedVelocity.Average(v => Math.Min(v.Y, 2));
            velocity.Z = queuedVelocity.Average(v => Math.Min(v.Z, 2));

            this.Velocity = velocity;

            if (IsSwiping)
                IsSwiping = false;

            if (!IsSwiping && (lastSwipe -= elapsedTime) < TimeSpan.Zero && Velocity.Length() > SwipeSpeedThreshold)
            {
                IsSwiping = true;
                lastSwipe = TimeSpan.FromSeconds(MaxSwipeInterval);
                queuedVelocity.Clear();
                queuedTimeStamps.Clear();
            }
        }
    }
}
