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
#endregion


namespace Isles.Graphics.Cameras
{
    public class TopDownCamera : ICamera
    {

        #region ICamera Members

        public Matrix View
        {
            get { throw new NotImplementedException(); }
        }

        public Matrix Projection
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
