#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Isles.Graphics.Vertices;
#endregion


namespace Isles.Graphics.Landscape
{
    public sealed class Terrain : IDisposable
    {
        public TerrainGeometry Geometry { get; private set; }
        public TerrainEffectCollection Effects { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }
        public ReadOnlyCollection<TerrainPatch> Patches { get; private set; }
        public int PatchTessellation { get; private set; }
        public object Tag { get; set; }
        public string Name { get; set; }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; UpdatePatchPositions(); }
        }

        private Vector3 position;


        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox box;

                box.Min = Geometry.BoundingBox.Min + position;
                box.Max = Geometry.BoundingBox.Max + position;

                return box;
            }
        }


        public Terrain(GraphicsDevice graphics, float step, int tessellationU, int tessellationV, int patchTessellation)
            : this(graphics, new TerrainGeometry(step, tessellationU, tessellationV), patchTessellation)
        { }

        public Terrain(GraphicsDevice graphics, TerrainGeometry geometry, int patchTessellation)
        {
            if (graphics == null || geometry == null)
                throw new ArgumentNullException();

            if (patchTessellation < 2 ||
                geometry.TessellationU % patchTessellation != 0 ||
                geometry.TessellationV % patchTessellation != 0)
            {
                throw new ArgumentOutOfRangeException();
            }


            Effects = new TerrainEffectCollection(this);
            PatchTessellation = patchTessellation;
            GraphicsDevice = graphics;
            Geometry = geometry;

            Geometry.Invalidate += (a, b) => Invalidate();

            // Create patches
            int xPatchCount = geometry.TessellationU / patchTessellation;
            int yPatchCount = geometry.TessellationV / patchTessellation;

            TerrainPatch[] patches = new TerrainPatch[xPatchCount * yPatchCount];

            int i = 0;

            for (int y = 0; y < yPatchCount; y++)
            {
                for (int x = 0; x < xPatchCount; x++)
                {
                    patches[i] = new TerrainPatch(graphics, geometry, x, y, patchTessellation);

                    i++;
                }
            }

            Patches = new ReadOnlyCollection<TerrainPatch>(patches);

            // Store these values in case they change
            tu = Geometry.TessellationU;
            tv = Geometry.TessellationV;
        }

        private int tu, tv;

        public void Invalidate()
        {
            // Make sure we don't change geometry tessellation
            if (Geometry.TessellationU != tu ||
                Geometry.TessellationV != tv)
            {
                throw new InvalidOperationException();
            }

            foreach (TerrainPatch patch in Patches)
            {
                patch.Invalidate();
            }
        }
        
        private void UpdatePatchPositions()
        {
            foreach (TerrainPatch patch in Patches)
            {
                patch.Position = position;
            }
        }


        public void Dispose()
        {
            foreach (TerrainPatch patch in Patches)
            {
                patch.Dispose();
            }
        }
    }
}
