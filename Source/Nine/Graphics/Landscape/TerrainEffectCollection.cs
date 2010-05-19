#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Landscape
{    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class TerrainEffectCollection : Collection<Effect>
    {
        private Terrain terrain;


        internal TerrainEffectCollection(Terrain terrain)
        {
            this.terrain = terrain;
        }

        protected override void InsertItem(int index, Effect item)
        {
            foreach (TerrainPatch patch in terrain.Patches)
            {
                patch.Effects.Insert(index, item);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            foreach (TerrainPatch patch in terrain.Patches)
            {
                patch.Effects.Remove(this[index]);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Effect item)
        {
            throw new NotImplementedException();
        }
    }
}
