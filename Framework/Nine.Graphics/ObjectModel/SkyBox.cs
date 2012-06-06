#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.Primitives;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines a skybox.
    /// </summary>
    [ContentSerializable]
    public class SkyBox : Drawable
    {
        /// <summary>
        /// Gets or sets the color of the skybox.
        /// </summary>
        public Vector3 Color
        {
            get { return material.Color; }
            set { material.Color = value; }
        }

        /// <summary>
        /// Gets or sets the skybox texture.
        /// </summary>
        public TextureCube Texture
        {
            get { return material.Texture; }
            set { material.Texture = value; }
        }

        protected override Material MaterialValue
        {
            get { return material; }
        }

        private Box cube;
        private SkyBoxMaterial material;

        public SkyBox(GraphicsDevice graphics)
        {
            material = new SkyBoxMaterial(graphics);
            cube = new Box(graphics) { InvertWindingOrder = true, Material = material, };
        }

        public override void Draw(DrawingContext context, Material material)
        {
            if (material is SkyBoxMaterial)
            {
                // Skybox cannot be rendered using other materials.
                cube.Draw(context, material);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                cube.Dispose();
        }
    }
}