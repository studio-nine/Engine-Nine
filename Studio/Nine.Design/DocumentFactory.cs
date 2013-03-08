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
using System.IO;
using Nine.Studio;
using System.ComponentModel.Composition;
using Nine.Content.Graphics.ParticleEffects;
using System.Text;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Graphics.ParticleEffects.Design
{
    [Export(typeof(IDocumentFactory))]
    public class ParticleEffectDocumentFactory : DocumentFactory<ParticleEffect> 
    {
        public ParticleEffectDocumentFactory()
        {
            DisplayName = Strings.ParticleEffect;
            Icon = Resources.ParticleEffect;
        }
    }
}
