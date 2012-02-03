#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Content.Pipeline.Graphics.ObjectModel
{
    [System.Windows.Markup.ContentProperty("Children")]
    partial class DisplayObjectContent
    {
        [ContentSerializer(Optional = true)]
        public virtual Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateTransform(); }
        }
        Vector3 position;

        /// <summary>
        /// Gets or sets the eular rotation in degrees.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public virtual Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        Vector3 rotation;

        [ContentSerializer(Optional = true)]
        public RotationOrder RotationOrder { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        Vector3 scale = Vector3.One;

        private void UpdateTransform()
        {
            Transform = Matrix.CreateScale(scale) *
                        MatrixHelper.CreateRotation(new Vector3(MathHelper.ToRadians(rotation.X)
                                                              , MathHelper.ToRadians(rotation.Y)
                                                              , MathHelper.ToRadians(rotation.Z))
                                                              , RotationOrder) *
                        Matrix.CreateTranslation(position);
        }
    }
}
