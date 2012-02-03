#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Content.Pipeline.Graphics.ObjectModel
{
    [ContentProperty("Material")]
    partial class DecalContent
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
            set { rotation = value; UpdateTransform(); }     
        }
        Vector3 rotation;

        [ContentSerializer(Optional = true)]
        public RotationOrder RotationOrder { get; set; }
                
        private void UpdateTransform()
        {
            Transform = MatrixHelper.CreateRotation(new Vector3(MathHelper.ToRadians(rotation.X)
                                                              , MathHelper.ToRadians(rotation.Y)
                                                              , MathHelper.ToRadians(rotation.Z))
                                                              , RotationOrder) *
                        Matrix.CreateTranslation(position);
        }
    }
}
