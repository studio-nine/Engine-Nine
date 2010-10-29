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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Custom vertex structure for drawing point sprite particles.
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    internal struct ParticleVertex : IVertexType
    {
        // Stores the starting position of the particle.
        public Vector3 Position;

        // Texture coordinates
        public Vector2 TextureCoordinates;

        // The time (in seconds) at which this particle was created.
        public float Time;
        
        // Four random values, used to make each particle look slightly different.
        public Color Random1;
        public Color Random2;

        public Vector3 Velocity;

        // Describe the layout of this vertex structure.
        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(28, VertexElementFormat.Color, VertexElementUsage.Color, 1),
            new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 2),
        };


        // Describe the size of this vertex structure.
        public static int SizeInBytes = 4 * (3 + 1 + 1 + 1 + 2 + 3);

        public VertexDeclaration VertexDeclaration
        {
            get { return new VertexDeclaration(VertexElements); }
        }
    }
}