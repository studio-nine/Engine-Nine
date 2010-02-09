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


namespace Isles.Graphics.ParticleEffects
{
    public abstract class ParticleEffect : GraphicsEffect
    {
        public Vector3 BillboardAxis { get; set; }
        public ParticleType ParticleType { get; set; }

        public ParticleEffect()
        {
            BillboardAxis = Vector3.UnitZ;
            ParticleType = ParticleType.PointSprite;
        }
    }
}
