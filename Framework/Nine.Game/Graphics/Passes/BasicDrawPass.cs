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

namespace Nine.Graphics.Passes
{
    public class BasicDrawPass : GraphicsPass<IDrawableView>
    {
        public Effect Effect { get; set; }
        public bool DrawTransparentObjects { get; set; }
        public bool DrawOpaqueObjects { get; set; }

        public BasicDrawPass()
        {
            DrawTransparentObjects = true;
            DrawOpaqueObjects = true;
        }

        public override void Draw(GraphicsContext context, ISpatialQuery<IDrawableView> drawables)
        {
            context.ModelBatch.Begin(context.View, context.Projection);
            context.PrimitiveBatch.Begin(context.View, context.Projection);

            if (DrawTransparentObjects)
                context.ParticleBatch.Begin(context.View, context.Projection);

            foreach (var drawable in drawables.FindAll(context.ViewFrustum))
            {
                if (drawable == null)
                    continue;

                IUpdateable updateable = drawable as IUpdateable;
                if (updateable != null)
                    updateable.Update(context.ElapsedTime);

                bool isTransparent = IsTransparent(drawable as IMaterial);
                bool shouldDraw = isTransparent ? DrawTransparentObjects : DrawOpaqueObjects;

                if (shouldDraw)
                {
                    if (Effect != null)
                        drawable.Draw(context, Effect);
                    else
                        drawable.Draw(context);
                }
            }

            context.ModelBatch.End();
            context.PrimitiveBatch.End();

            if (DrawTransparentObjects)
                context.ParticleBatch.End();
        }

        private bool IsTransparent(IMaterial drawable)
        {
            if (drawable == null)
                return false;        
            return drawable.IsTransparent;
        }
    }
}