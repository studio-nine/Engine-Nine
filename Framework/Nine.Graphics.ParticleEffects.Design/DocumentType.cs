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
using Nine.Content.Pipeline.Graphics.ParticleEffects;
using System.Text;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Graphics.ParticleEffects.Design
{
    [Export(typeof(IDocumentType))]
    public class ParticleEffectDocumentType : IDocumentType
    {
        public string DisplayName
        {
            get { return Strings.ParticleEffect; }
        }

        public string Description
        {
            get { return null; }
        }

        public string FileExtension
        {
            get { return ".xml"; }
        }
        
        public object Icon
        {
            get { return Resources.ParticleEffect; }
        }

        public Type DefaultSerializer
        {
            get { return null; }
        }

        public object CreateDocumentObject()
        {
            return new ParticleEffect();
        }

        public bool CheckSupported(Stream stream)
        {
            int length = 16;
            byte[] header = new byte[length];
            length = stream.Read(header, 0, length);
            if (length <= 0)
                return false;

            string xml = Encoding.Default.GetString(header);
            return xml.ToLowerInvariant().StartsWith("<?xml");
        }
    }
}
