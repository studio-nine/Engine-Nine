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
    public class KinectSkeleton : Skeleton
    {
        Runtime Runtime;

        public const int BoneCount = (int)JointID.Count;
        public int Index { get; internal set; }
        public SkeletonData SkeletonData { get; internal set; }
        public SkeletonFrame SkeletonFrame { get; internal set; }
        public bool IsTracked { get { return State == SkeletonTrackingState.Tracked; } }
        public SkeletonTrackingState State { get; internal set; }
        public object Tag { get; set; }

        public override Matrix[] BoneTransforms
        {
            get { return boneTransforms; }
        }
        Matrix[] boneTransforms;

        public override ReadOnlyCollection<int> ParentBones
        {
            get { return Kinect.ParentBones; }
        }
        
        internal KinectSkeleton(Runtime runtime)
        {
            Runtime = runtime;
            boneTransforms = new Matrix[(int)JointID.Count];
            BoneNames = new ReadOnlyCollection<string>(Enumerable.Range(0, (int)JointID.Count).Select(i => ((JointID)i).ToString()).ToList());
        }

        public int GetBone(JointID bone)
        {
            return (int)bone;
        }

        public Matrix GetAbsoluteBoneTransform(JointID bone)
        {
            return this.GetAbsoluteBoneTransform((int)bone);
        }

        public Matrix GetBoneTransform(JointID bone)
        {
            return this.GetBoneTransform((int)bone);
        }

        public Vector2 Project(JointID bone, float width, float height)
        {
            Matrix jointTransform = GetAbsoluteBoneTransform(bone);
            Vector jointPosition = new Vector() { X = jointTransform.M41, Y = jointTransform.M42, Z = jointTransform.M43, W = jointTransform.M44 };

            float depthX, depthY;
            Runtime.SkeletonEngine.SkeletonToDepthImage(jointPosition, out depthX, out depthY);
            depthX = Math.Max(0, Math.Min(depthX * 320, 320));  //convert to 320, 240 space
            depthY = Math.Max(0, Math.Min(depthY * 240, 240));  //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            Runtime.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            return new Vector2((int)(width * colorX / 640.0), (int)(height * colorY / 480));
        }
    }
}
