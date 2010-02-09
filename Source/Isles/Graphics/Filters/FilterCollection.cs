#region File Description
//-----------------------------------------------------------------------------
// BloomComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics.Filters
{
    public sealed class FilterCollection : Collection<Filter>
    {
        public Texture2D Process(GraphicsDevice graphics, Texture2D input)
        {
            for (int i = 0; i < Count; i++)
            {
                input = this[i].Process(graphics, input);
            }

            return input;
        }

        public void Draw(GraphicsDevice graphics)
        {
            Draw(graphics, null);
        }

        public void Draw(GraphicsDevice graphics, Texture2D input)
        {
            for (int i = 0; i < Count - 1; i++)
            {
                input = this[i].Process(graphics, input);
            }

            if (Count > 0)
                this[Count - 1].Draw(graphics, input);
        }

        public void Draw(GraphicsDevice graphics, Texture2D input, Rectangle destination)
        {
            for (int i = 0; i < Count - 1; i++)
            {
                input = this[i].Process(graphics, input);
            }

            if (Count > 0)
                this[Count - 1].Draw(graphics, input, destination);
        }
    }
}

