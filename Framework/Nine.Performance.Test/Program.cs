#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Nine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nine.Test
{
    class Program
    {
        public static void Main(string[] args)
        {
            OctreeObjectManagerTest test = new OctreeObjectManagerTest();
            test.AddQueryUpdateRemoveTest();
        }
    }
}
