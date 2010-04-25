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


namespace Isles.Graphics.Effects
{
    public sealed class SplatterTextureCollection : IEnumerable<Texture2D>
    {
        Texture2D[] textures;
        SplatterEffect effect;

        internal SplatterTextureCollection(SplatterEffect effect)
        {
            this.effect = effect;
            this.textures = new Texture2D[SplatterEffect.MaxLayers];
        }

        public Texture2D this[int index]
        {
            get { return textures[index]; }
            set { textures[index] = value; effect.UpdateTexture(); }
        }

        public IEnumerator<Texture2D> GetEnumerator()
        {
            return (textures as IEnumerable<Texture2D>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return textures.GetEnumerator();
        }
    }


    public partial class SplatterEffect
    {
        public const int MaxLayers = 4;

        public SplatterTextureCollection Textures { get; private set; }


        public bool FogEnabled
        {
            get { return FogMask > 0.5f; }
            set { FogMask = (value ? 1.0f : 0.0f); }
        }


        public Matrix View
        {
            get { return ViewMatrix; }
            set { ViewMatrix = value; EyePosition = Matrix.Invert(value).Translation; }
        }

        
        public SplatterEffect(GraphicsDevice graphicsDevice) : this(graphicsDevice, null) { }
        
        public SplatterEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) : 
                base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();

            Textures = new SplatterTextureCollection(this);
        }

        internal void UpdateTexture() 
        {
            TextureX = Textures[0];
            TextureY = Textures[1];
            TextureZ = Textures[2];
            TextureW = Textures[3];

            Vector4 mask;

            mask.X = (Textures[0] != null ? 1.0f : 0.0f);
            mask.Y = (Textures[1] != null ? 1.0f : 0.0f);
            mask.Z = (Textures[2] != null ? 1.0f : 0.0f);
            mask.W = (Textures[3] != null ? 1.0f : 0.0f);

            Mask = mask;
        }
    }
}
