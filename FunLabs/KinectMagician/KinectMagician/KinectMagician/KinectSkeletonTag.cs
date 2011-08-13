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
    public class KinectSkeletonTag
    {
        public KinectGestureTracker LeftHand;
        public KinectGestureTracker RightHand;

        public bool LeftHandOnFire = true;
        public bool RightHandOnFire = true;
    }
}
