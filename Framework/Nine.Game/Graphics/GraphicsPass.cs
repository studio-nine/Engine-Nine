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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Represents a pass when drawing the objects.
    /// </summary>
    public abstract class GraphicsPass
    {
        public abstract void Draw(GraphicsContext context, ISpatialQuery<object> drawables);
    }

    /// <summary>
    /// Represents a pass when drawing the objects.
    /// </summary>
    public abstract class GraphicsPass<T> : GraphicsPass
    {
        private SpatialQuery<object, T> query;

        protected GraphicsPass()
        {
            query = new SpatialQuery<object, T>();
            query.Filter = d => d is T;
            query.Converter = d => (T)d;
            query.InnerQueries.Add(null);
        }

        public override void Draw(GraphicsContext context, ISpatialQuery<object> drawables)
        {
            query.InnerQueries[0] = drawables;
            Draw(context, query);
        }

        public abstract void Draw(GraphicsContext context, ISpatialQuery<T> drawables);
    }
}