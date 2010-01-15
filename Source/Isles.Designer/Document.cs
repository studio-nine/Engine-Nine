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
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Designer
{
    /// <summary>
    /// Isles designer document object model interface.
    /// </summary>
    public interface IDocument
    {
        string DocumentType { get; }
        string DocumentName { get; }

        bool IsModified { get; }

        void Load(Stream input);
        void Save(Stream output);
        void Build();

        void Initialize(IDesigner designer);
    }
}