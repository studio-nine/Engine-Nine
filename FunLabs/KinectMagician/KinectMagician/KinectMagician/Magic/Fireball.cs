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
using Microsoft.Xna.Framework.Content;
using System.Collections.ObjectModel;
using Nine.Graphics;
using Nine.Animations;
using Nine.Graphics.ParticleEffects;

namespace Nine
{
    public class Fireball : Magic
    {
        public Fireball(ContentManager content) : base(content, "Fireworks") { }

        protected override void OnInitialize(KinectSkeleton skeleton, KinectSkeletonTag tag)
        {
            while (ParticleEffect.ActiveEmitters.Count < KinectSkeleton.BoneCount)
            {
                ParticleEffect.Trigger().Enabled = false;
            }
        }

        protected override bool Update(TimeSpan elapsedTime, KinectSkeleton skeleton, KinectSkeletonTag tag)
        {
            if (skeleton.IsTracked)
            {
                ParticleEffect.ActiveEmitters[skeleton.Index * 2 + 0].Position = GetDisplayPosition(skeleton, JointID.HandRight);
                ParticleEffect.ActiveEmitters[skeleton.Index * 2 + 1].Position = GetDisplayPosition(skeleton, JointID.HandLeft);
            }
            /*
            if (tag.LeftHand.IsSwiping)
                tag.LeftHandOnFire = !tag.LeftHandOnFire;
            if (tag.RightHand.IsSwiping)
                tag.RightHandOnFire = !tag.RightHandOnFire;
            */
            return tag.RightHandOnFire || tag.LeftHandOnFire;
        }

        public override void Enable(KinectSkeleton skeleton)
        {
            KinectSkeletonTag tag = (KinectSkeletonTag)skeleton.Tag;

            ParticleEffect.ActiveEmitters[skeleton.Index * 2 + 0].Enabled = tag.RightHandOnFire;
            ParticleEffect.ActiveEmitters[skeleton.Index * 2 + 1].Enabled = tag.LeftHandOnFire;
        }

        public override void Disable(KinectSkeleton skeleton)
        {
            ParticleEffect.ActiveEmitters[skeleton.Index * 2 + 0].Enabled = false;
            ParticleEffect.ActiveEmitters[skeleton.Index * 2 + 1].Enabled = false;
        }
    }
}
