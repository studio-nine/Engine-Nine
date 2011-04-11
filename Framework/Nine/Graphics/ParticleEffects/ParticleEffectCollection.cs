#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Represents a collection of particle effects.
    /// </summary>
    [EditorBrowsable()]
    public class ParticleEffectCollection : Collection<ParticleEffect>
    {
        internal bool ClearTriggerList = false;
        internal bool EnsureHasTrigger = false;

        protected override void InsertItem(int index, ParticleEffect item)
        {
            if (ClearTriggerList)
                item.TriggerList.Clear();
            if (EnsureHasTrigger && item.TriggerList.Count == 0)
                item.Trigger();
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ParticleEffect item)
        {
            if (ClearTriggerList)
                item.TriggerList.Clear();
            if (EnsureHasTrigger && item.TriggerList.Count == 0)
                item.Trigger();
            base.SetItem(index, item);
        }
    }
}