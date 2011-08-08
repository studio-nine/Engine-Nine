#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using Nine.Animations;
#endregion

namespace Nine.Graphics.ObjectModel
{
    #region Terrain
    /// <summary>
    /// Defines a view of terrain.
    /// </summary>
    public class Terrain : Drawable, IMaterial, ILightable, IEnumerable<Drawable>
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        #region Heightmap
        [ContentSerializer]
        public Heightmap Heightmap { get; internal set; }
        #endregion

        #region Surface
        public DrawableSurface Surface
        {
            get 
            {
                if (surface == null && Heightmap != null)
                {
                    surface = new DrawableSurface(GraphicsDevice, Heightmap, PatchTessellation);
                    surface.Position = Transform.Translation;
                    patches = surface.Patches.Select(p => new TerrainPatch() { SurfacePatch = p, Terrain = this }).ToArray();
                }
                return surface; 
            }
        }
        private DrawableSurface surface;
        private TerrainPatch[] patches;
        #endregion

        #region Transform
        protected override void OnTransformChanged()
        {
            if (Surface != null)
            {
                surface.Position = Transform.Translation;
                patches.ForEach(p => p.NotifyBoundingBoxChanged());
            }
            base.OnTransformChanged();
        }

        [ContentSerializer]
        internal int PatchTessellation = 8;

        public override BoundingBox BoundingBox
        {
            get { return Surface != null ? Surface.BoundingBox : new BoundingBox(); }
        }
        #endregion

        #region Material
        [ContentSerializer]
        public IEffectInstance Effect { get; internal set; }

        public bool IsTransparent { get { return false; } }
        bool ILightable.CastShadow { get { return false; } }
        public bool ReceiveShadow { get; set; }

        internal Texture2D diffuseTexture;
        #endregion

        public override void Draw(GraphicsContext context) { }

        internal Terrain(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.GraphicsDevice = graphics;
        }

        public Terrain(GraphicsDevice graphics, Heightmap heightmap) : this(graphics, heightmap, 8)
        {

        }

        public Terrain(GraphicsDevice graphics, Heightmap heightmap, int patchTessellation)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (heightmap == null)
                throw new ArgumentNullException("heightmap");

            this.GraphicsDevice = graphics;
            this.Heightmap = heightmap;
            this.PatchTessellation = patchTessellation;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Drawable> GetEnumerator()
        {
            return patches != null ? patches.OfType<Drawable>().GetEnumerator() : Enumerable.Empty<Drawable>().GetEnumerator();
        }
    }
    #endregion

    #region TerrainPatch
    [NotContentSerializable]
    class TerrainPatch : Drawable, IMaterial, ILightable
    {
        internal Terrain Terrain;
        internal DrawableSurfacePatch SurfacePatch;

        public override BoundingBox BoundingBox
        {
            get { return SurfacePatch.BoundingBox; }
        }

        public IEffectInstance Effect { get { return Terrain.Effect; } }
        public bool IsTransparent { get { return Terrain.IsTransparent; } }
        public bool CastShadow { get { return false; } }
        public bool ReceiveShadow { get { return Terrain.ReceiveShadow; } }

        public void NotifyBoundingBoxChanged()
        {
            OnBoundingBoxChanged();
        }

        public override void Draw(GraphicsContext context)
        {
            context.ModelBatch.DrawSurface(SurfacePatch, Effect);
        }

        public override void Draw(GraphicsContext context, Effect effect)
        {
            effect.SetTexture(Terrain.diffuseTexture ?? Terrain.Effect.Effect.GetTexture());
            context.ModelBatch.DrawSurface(SurfacePatch, effect);
        }
    }
    #endregion
}