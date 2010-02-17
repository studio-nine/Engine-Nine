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
    public sealed class BillboardVisual : IParticleVisual
    {
        #region IParticleVisual Members

        public void Draw(GraphicsDevice graphics, ParticleVertex[] particles, Matrix view, Matrix projection)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
