/////////////////////////////////////////////////////////////////////////
//
// This module contains code to do Kinect NUI initialization and
// processing and also to display NUI streams on screen.
//
// Copyright ?Microsoft Corporation.  All rights reserved.  
// This code is licensed under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Animations;
using Nine.Graphics;
using System.Collections.ObjectModel;

namespace Nine
{
    public class Kinect : IUpdateable, IDisposable
    {
        int totalFrames = 0;
        int lastFrames = 0;
        DateTime lastTime = DateTime.MaxValue;
        KinectSkeleton[] skeletons;

        byte[] colorBits;
        byte[] depthBits;
        Color[] depthFrame = new Color[320 * 240];
        Color[] colorFrame = new Color[640 * 480];

        public const int SkeletonCount = 6;

        public Runtime Runtime { get; private set; }
        public float FrameRate { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public Texture2D DepthTexture { get; private set; }
        public Texture2D ColorTexture { get; private set; }
        public ReadOnlyCollection<KinectSkeleton> Skeletons { get; private set; }

        public Kinect(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            Runtime = new Runtime();

            try
            {
                Runtime.Initialize(RuntimeOptions.UseDepthAndPlayerIndex | RuntimeOptions.UseSkeletalTracking | RuntimeOptions.UseColor);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException("Runtime initialization failed. Please make sure Kinect device is plugged in.", e);
            }


            try
            {
                Runtime.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                Runtime.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException("Failed to open stream. Please make sure to specify a supported image type and resolution.", e);
            }

            lastTime = DateTime.Now;

            DepthTexture = new Texture2D(GraphicsDevice, 320, 240);
            ColorTexture = new Texture2D(GraphicsDevice, 640, 480);
            skeletons = Enumerable.Range(0, SkeletonCount).Select(i => new KinectSkeleton(Runtime) { Index = i }).ToArray();
            Skeletons = new ReadOnlyCollection<KinectSkeleton>(skeletons);

            Runtime.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
            Runtime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_ColorFrameReady);
            Runtime.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
        }

        // Converts a 16-bit grayscale depth frame which includes player indexes into a 32-bit frame
        // that displays different players in different colors
        Color[] convertDepthFrame(byte[] depthFrame16)
        {
            for (int i16 = 0, i = 0; i16 < depthFrame16.Length && i < depthFrame.Length; i16 += 2, i++)
            {
                int player = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16+1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                float intensity = (float)(255 - (255 * realDepth / 0x0fff)) / 255;

                depthFrame[i] = new Color(intensity, intensity, intensity);
            }
            return depthFrame;
        }

        void nui_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            depthBits = e.ImageFrame.Image.Bits;

            ++totalFrames;

            DateTime cur = DateTime.Now;
            if (cur.Subtract(lastTime) > TimeSpan.FromSeconds(1))
            {
                int frameDiff = totalFrames - lastFrames;
                lastFrames = totalFrames;
                lastTime = cur;
                FrameRate = frameDiff;
            }
        }

        private Matrix GetRelativeBoneTransform(SkeletonData data, Joint joint)
        {
            if (ParentBones[(int)joint.ID] < 0)
                return Matrix.CreateTranslation(joint.Position.X, joint.Position.Y, joint.Position.Z);

            var parentJoint = data.Joints[(JointID)ParentBones[(int)joint.ID]];
            return Matrix.CreateTranslation(joint.Position.X - parentJoint.Position.X,
                                            joint.Position.Y - parentJoint.Position.Y,
                                            joint.Position.Z - parentJoint.Position.Z);
        }

        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = e.SkeletonFrame;
            int iSkeleton = 0;

            foreach (SkeletonData data in skeletonFrame.Skeletons)
            {
                var skeleton = skeletons[iSkeleton];
                skeleton.State = data.TrackingState;
                skeleton.SkeletonData = data;
                skeleton.SkeletonFrame = skeletonFrame;
                if (SkeletonTrackingState.Tracked == data.TrackingState)
                {
                    for (int i = 0; i < (int)JointID.Count; i++)
                    {
                        skeleton.BoneTransforms[i] = GetRelativeBoneTransform(data, data.Joints[(JointID)i]);
                    }
                    
                    //skeleton.BoneTransforms[0].M41 += data.Position.X;
                    //skeleton.BoneTransforms[0].M42 += data.Position.Y;
                    //skeleton.BoneTransforms[0].M43 += data.Position.Z;
                }
                iSkeleton++;
            }
        }

        Color[] convertColorFrame(byte[] colors)
        {
            for (int i32 = 0, i = 0; i32 < colors.Length && i < colorFrame.Length; i32 += 4, i++)
            {
                colorFrame[i] = new Color(colors[i32 + 2], colors[i32 + 1], colors[i32], 255);
            }
            return colorFrame;
        }

        void nui_ColorFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            colorBits = e.ImageFrame.Image.Bits;
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (colorBits != null)
            {
                ColorTexture.SetData<Color>(convertColorFrame(colorBits));
                colorBits = null;
            }
            if (depthBits != null)
            {
                DepthTexture.SetData<Color>(convertDepthFrame(depthBits));
                depthBits = null;
            }
        }

        public void Dispose()
        {
            if (Runtime != null)
                Runtime.Uninitialize();
        }

        internal static ReadOnlyCollection<int> ParentBones = new ReadOnlyCollection<int>(new int[]
        {                  
            -1,//HipCenter, = 0,
            (int)JointID.HipCenter,//Spine = 1,
            (int)JointID.Spine,//ShoulderCenter, = 2,
            (int)JointID.ShoulderCenter,//Head = 3,
            (int)JointID.ShoulderCenter,//ShoulderLeft = 4,
            (int)JointID.ShoulderLeft,//ElbowLeft = 5,
            (int)JointID.ElbowLeft,//WristLeft = 6,
            (int)JointID.WristLeft,//HandLeft = 7,
            (int)JointID.ShoulderCenter,//ShoulderRight = 8,
            (int)JointID.ShoulderRight,//ElbowRight = 9,
            (int)JointID.ElbowRight,//WristRight = 10,
            (int)JointID.WristRight,//HandRight = 11,
            (int)JointID.HipCenter,//HipLeft = 12,
            (int)JointID.HipLeft,//KneeLeft = 13,
            (int)JointID.KneeLeft,//AnkleLeft = 14,
            (int)JointID.AnkleLeft,//FootLeft = 15,
            (int)JointID.HipCenter,//HipRight = 16,
            (int)JointID.HipRight,//KneeRight = 17,
            (int)JointID.KneeRight,//AnkleRight = 18,
            (int)JointID.AnkleRight,//FootRight = 19,
        });
    }
}
