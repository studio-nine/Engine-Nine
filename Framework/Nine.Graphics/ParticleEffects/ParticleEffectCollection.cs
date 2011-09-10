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
        protected override void InsertItem(int index, ParticleEffect item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.Triggers.Count > 0)
                throw new ArgumentException(Strings.ParticleEffectAlreadyTriggered);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ParticleEffect item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.Triggers.Count > 0)
                throw new ArgumentException(Strings.ParticleEffectAlreadyTriggered);
            base.SetItem(index, item);
        }
    }
}