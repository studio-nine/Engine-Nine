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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Transitions
{
    public sealed class ShakeTransition : ITransition<float>, ITransition<Vector2>, ITransition<Vector3>
    {
        public float ShakeAmount { get; set; }
        public TimeSpan Duration { get; set; }


        public ShakeTransition()
        {
            ShakeAmount = 1.0f;
            Duration = TimeSpan.FromSeconds(1);
        }


        float ITransition<float>.Update(GameTime time)
        {
            return 0;
        }


        Vector2 ITransition<Vector2>.Update(GameTime time)
        {
            return Vector2.Zero;
        }


        Vector3 ITransition<Vector3>.Update(GameTime time)
        {
            return Vector3.Zero;
        }
    }
}