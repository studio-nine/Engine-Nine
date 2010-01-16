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


namespace Isles.Game
{
    public interface ITickObject
    {
        void Update(GameTime gameTime);
    }

    public interface IDisplayObject : ITickObject
    {
        Matrix Transform { get; set; }

        void Draw(GameTime gameTime, Matrix view, Matrix projection);
    }
}