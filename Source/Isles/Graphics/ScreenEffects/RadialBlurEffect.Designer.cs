// -----------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:v2.0.50727
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// -----------------------------------------------------------------------------

namespace Isles.Graphics.ScreenEffects
{
	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

    partial class RadialBlurEffect : Effect
    {
        private void InitializeComponent()
        {
            this._Center = Parameters["Center"];
            this._BlurAmount = Parameters["BlurAmount"];

        }

		#region Properties

        private EffectParameter _Center;

        /// <summary>
        /// Gets or sets the center of the radial blur.
        /// </summary>
        public Vector2 Center
        {
            get { return _Center.GetValueVector2(); }
            set { _Center.SetValue(value); }
        }

        private EffectParameter _BlurAmount;

        /// <summary>
        /// Gets or sets the blur amount.
        /// </summary>
        public float BlurAmount
        {
            get { return _BlurAmount.GetValueSingle(); }
            set { _BlurAmount.SetValue(value); }
        }


		#endregion

        #region SharedEffect
        static Effect sharedEffect = null;

        static Effect GetSharedEffect(GraphicsDevice graphics)
        {
            if (sharedEffect == null)
                sharedEffect = new Effect(graphics, effectCode);
            
            return sharedEffect;
        }
        #endregion

        #region ByteCode
        static byte[] effectCode = null;

        static RadialBlurEffect()
        {
#if XBOX360
            effectCode = new byte[] 
            {
0xBC, 0xF0, 0x0B, 0xCF, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x09, 0x01, 0x00, 0x00, 0x01, 0x20, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 
0x00, 0x00, 0x00, 0x01, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 
0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x53, 0x61, 0x73, 0x55, 0x69, 0x44, 0x65, 0x73, 
0x63, 0x72, 0x69, 0x70, 0x74, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x43, 0x65, 0x6E, 0x74, 0x65, 0x72, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 
0x00, 0x00, 0x00, 0x01, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x9C, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x53, 0x61, 0x73, 0x55, 0x69, 0x44, 0x65, 0x73, 0x63, 0x72, 0x69, 0x70, 
0x74, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x42, 0x6C, 0x75, 0x72, 0x41, 0x6D, 0x6F, 0x75, 0x6E, 0x74, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xDC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x0F, 0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x53, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 
0x50, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x52, 0x61, 0x64, 0x69, 0x61, 0x6C, 0x42, 0x6C, 0x75, 0x72, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x88, 0x00, 0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0xC4, 0x00, 0x00, 0x00, 0xD8, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x01, 0x08, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x5D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF4, 0x00, 0x00, 0x00, 0xF0, 0x00, 0x00, 0x00, 0x02, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x1E, 0x47, 0x65, 0x74, 0x73, 0x20, 0x6F, 0x72, 0x20, 0x73, 0x65, 0x74, 0x73, 
0x20, 0x74, 0x68, 0x65, 0x20, 0x62, 0x6C, 0x75, 0x72, 0x20, 0x61, 0x6D, 0x6F, 0x75, 0x6E, 0x74, 0x2E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 
0x00, 0x00, 0x00, 0x2C, 0x47, 0x65, 0x74, 0x73, 0x20, 0x6F, 0x72, 0x20, 0x73, 0x65, 0x74, 0x73, 0x20, 0x74, 0x68, 0x65, 0x20, 0x63, 0x65, 0x6E, 
0x74, 0x65, 0x72, 0x20, 0x6F, 0x66, 0x20, 0x74, 0x68, 0x65, 0x20, 0x72, 0x61, 0x64, 0x69, 0x61, 0x6C, 0x20, 0x62, 0x6C, 0x75, 0x72, 0x2E, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x38, 
0x10, 0x2A, 0x11, 0x00, 0x00, 0x00, 0x01, 0x54, 0x00, 0x00, 0x02, 0xE4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x01, 0x08, 
0x00, 0x00, 0x01, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0xD3, 
0xFF, 0xFF, 0x03, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00, 0x58, 
0x00, 0x02, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x74, 0x00, 0x00, 0x00, 0x84, 0x00, 0x02, 0x00, 0x00, 
0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x8C, 0x00, 0x00, 0x00, 0x9C, 0x00, 0x00, 0x00, 0xAC, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 
0x00, 0x00, 0x00, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x42, 0x6C, 0x75, 0x72, 0x41, 0x6D, 0x6F, 0x75, 0x6E, 0x74, 0x00, 0xAB, 0x00, 0x00, 0x00, 0x03, 
0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x43, 0x65, 0x6E, 0x74, 0x65, 0x72, 0x00, 0xAB, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x54, 0x65, 0x78, 0x74, 
0x75, 0x72, 0x65, 0x53, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x00, 0xAB, 0x00, 0x04, 0x00, 0x0C, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x70, 0x73, 0x5F, 0x33, 0x5F, 0x30, 0x00, 0x32, 0x2E, 0x30, 0x2E, 0x38, 0x32, 0x37, 0x35, 0x2E, 0x30, 0x00, 0xAB, 0xAB, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x01, 0xF8, 0x00, 0x20, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x02, 0x64, 
0x10, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x21, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 
0x00, 0x00, 0x30, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x3B, 0xDA, 0x73, 0x7E, 0x3C, 0x5A, 0x73, 0x7E, 0x3C, 0xA3, 0xD6, 0x9E, 0x3D, 0x80, 0x00, 0x00, 0x3C, 0xDA, 0x73, 0x7E, 
0x3D, 0x08, 0x88, 0x2F, 0x3D, 0x23, 0xD6, 0x9E, 0x3D, 0x3F, 0x25, 0x0E, 0x3D, 0x5A, 0x73, 0x7E, 0x3D, 0x75, 0xC1, 0xEE, 0x3D, 0x88, 0x88, 0x2F, 
0x3D, 0x96, 0x2F, 0x67, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3D, 0xA3, 0xD6, 0x9E, 
0x3D, 0xB1, 0x7D, 0xD6, 0x3D, 0xBF, 0x25, 0x0E, 0x3D, 0xCC, 0xCC, 0x46, 0x00, 0x00, 0x60, 0x05, 0x60, 0x0B, 0x12, 0x00, 0x12, 0x00, 0x00, 0x00, 
0x05, 0x54, 0x60, 0x11, 0x60, 0x17, 0x12, 0x00, 0x12, 0x00, 0x05, 0x55, 0x01, 0x55, 0x50, 0x1D, 0x00, 0x00, 0x12, 0x00, 0xC4, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x60, 0x22, 0x60, 0x28, 0x12, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x2E, 0x00, 0x00, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 
0xC8, 0x0F, 0x00, 0x01, 0x00, 0x6C, 0x8F, 0x6C, 0x0B, 0x01, 0xFF, 0xFE, 0xC8, 0x0F, 0x00, 0x03, 0x00, 0x6C, 0x8F, 0x6C, 0x0B, 0x01, 0xFD, 0xFE, 
0xC8, 0x0F, 0x00, 0x04, 0x00, 0x6C, 0x8F, 0x6C, 0x0B, 0x01, 0xFC, 0xFE, 0xC8, 0x03, 0x00, 0x02, 0x02, 0x6D, 0x6D, 0x00, 0xA0, 0x00, 0x00, 0x00, 
0xC8, 0x07, 0x00, 0x05, 0x00, 0x6C, 0xBE, 0x6C, 0x0B, 0x01, 0xFB, 0xFE, 0xC8, 0x0C, 0x00, 0x00, 0x00, 0xAC, 0xB1, 0x71, 0xCB, 0x02, 0x05, 0x00, 
0xC8, 0x0F, 0x00, 0x0E, 0x00, 0x70, 0xCC, 0xAD, 0xCB, 0x02, 0x05, 0x00, 0xC8, 0x0F, 0x00, 0x0C, 0x00, 0x7D, 0xC1, 0xA0, 0xCB, 0x02, 0x04, 0x00, 
0xC8, 0x0F, 0x00, 0x0B, 0x00, 0xA0, 0x1C, 0x7D, 0xCB, 0x02, 0x04, 0x00, 0xC8, 0x0F, 0x00, 0x08, 0x00, 0x7D, 0xC1, 0xA0, 0xCB, 0x02, 0x03, 0x00, 
0xC8, 0x0F, 0x00, 0x07, 0x00, 0xA0, 0x1C, 0x7D, 0xCB, 0x02, 0x03, 0x00, 0xC8, 0x0F, 0x00, 0x04, 0x00, 0x7D, 0xC1, 0xA0, 0xCB, 0x02, 0x01, 0x00, 
0xC8, 0x0F, 0x00, 0x03, 0x00, 0xA0, 0x1C, 0x7D, 0xCB, 0x02, 0x01, 0x00, 0x04, 0x08, 0x10, 0x61, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0xB8, 0x08, 0x20, 0x81, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0xAC, 0x08, 0x30, 0x61, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0x10, 0x08, 0x40, 0x81, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0x04, 0x08, 0x50, 0xE1, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0xB8, 0x08, 0x61, 0x01, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0xAC, 0x08, 0x70, 0xE1, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0x10, 0x08, 0x81, 0x01, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0x04, 0x08, 0x91, 0x61, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0xB8, 0x08, 0xA1, 0x81, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0xAC, 0x08, 0xB1, 0x61, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0x10, 0x08, 0xC1, 0x81, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0x04, 0x08, 0xD1, 0xC1, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0xB8, 0x08, 0xE1, 0xC1, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0xAC, 0x08, 0xF0, 0x01, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0x10, 0x08, 0x00, 0x01, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x0F, 0x00, 0x00, 
0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x0E, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x0D, 0x00, 
0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x0C, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x0B, 0x00, 
0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x0A, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x09, 0x00, 
0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x08, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x07, 0x00, 
0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x06, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x05, 0x00, 
0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x04, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x03, 0x00, 
0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x02, 0x00, 0xC8, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x01, 0x00, 
0xC8, 0x0F, 0x80, 0x00, 0x00, 0x00, 0x1B, 0x00, 0xA1, 0x00, 0xFB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  
            };
#else
            effectCode = new byte[] 
            {
0xCF, 0x0B, 0xF0, 0xBC, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x09, 0xFF, 0xFE, 0x20, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 
0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x53, 0x61, 0x73, 0x55, 0x69, 0x44, 0x65, 0x73, 
0x63, 0x72, 0x69, 0x70, 0x74, 0x69, 0x6F, 0x6E, 0x00, 0xB3, 0xF4, 0x01, 0x07, 0x00, 0x00, 0x00, 0x43, 0x65, 0x6E, 0x74, 0x65, 0x72, 0x00, 0x01, 
0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x9C, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x53, 0x61, 0x73, 0x55, 0x69, 0x44, 0x65, 0x73, 0x63, 0x72, 0x69, 0x70, 
0x74, 0x69, 0x6F, 0x6E, 0x00, 0xB3, 0xF4, 0x01, 0x0B, 0x00, 0x00, 0x00, 0x42, 0x6C, 0x75, 0x72, 0x41, 0x6D, 0x6F, 0x75, 0x6E, 0x74, 0x00, 0x05, 
0x0A, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0xDC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x0F, 0x00, 0x00, 0x00, 0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x53, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x00, 0x18, 0x03, 0x00, 0x00, 0x00, 
0x0F, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 
0x50, 0x30, 0x00, 0x01, 0x0B, 0x00, 0x00, 0x00, 0x52, 0x61, 0x64, 0x69, 0x61, 0x6C, 0x42, 0x6C, 0x75, 0x72, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x88, 0x00, 0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0xC4, 0x00, 0x00, 0x00, 0xD8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x93, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF4, 0x00, 0x00, 0x00, 0xF0, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x1E, 0x00, 0x00, 0x00, 0x47, 0x65, 0x74, 0x73, 0x20, 0x6F, 0x72, 0x20, 0x73, 0x65, 0x74, 0x73, 
0x20, 0x74, 0x68, 0x65, 0x20, 0x62, 0x6C, 0x75, 0x72, 0x20, 0x61, 0x6D, 0x6F, 0x75, 0x6E, 0x74, 0x2E, 0x00, 0x3F, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x2C, 0x00, 0x00, 0x00, 0x47, 0x65, 0x74, 0x73, 0x20, 0x6F, 0x72, 0x20, 0x73, 0x65, 0x74, 0x73, 0x20, 0x74, 0x68, 0x65, 0x20, 0x63, 0x65, 0x6E, 
0x74, 0x65, 0x72, 0x20, 0x6F, 0x66, 0x20, 0x74, 0x68, 0x65, 0x20, 0x72, 0x61, 0x64, 0x69, 0x61, 0x6C, 0x20, 0x62, 0x6C, 0x75, 0x72, 0x2E, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x0B, 0x00, 0x00, 
0x00, 0x02, 0xFF, 0xFF, 0xFE, 0xFF, 0x32, 0x00, 0x43, 0x54, 0x41, 0x42, 0x1C, 0x00, 0x00, 0x00, 0x93, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 
0x02, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x8C, 0x00, 0x00, 0x00, 0x44, 0x00, 0x00, 0x00, 0x02, 0x00, 0x0F, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x4C, 0x00, 0x00, 0x00, 0x5C, 0x00, 0x00, 0x00, 0x6C, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 
0x7C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x43, 0x65, 0x6E, 0x74, 0x65, 0x72, 0x00, 0xAB, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x53, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x00, 0xAB, 0x04, 0x00, 0x0C, 0x00, 0x01, 0x00, 0x01, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x73, 0x5F, 0x32, 0x5F, 0x30, 0x00, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 
0x20, 0x28, 0x52, 0x29, 0x20, 0x48, 0x4C, 0x53, 0x4C, 0x20, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x20, 0x43, 0x6F, 0x6D, 0x70, 0x69, 0x6C, 0x65, 
0x72, 0x20, 0x39, 0x2E, 0x32, 0x36, 0x2E, 0x39, 0x35, 0x32, 0x2E, 0x32, 0x38, 0x34, 0x34, 0x00, 0xFE, 0xFF, 0xB0, 0x01, 0x50, 0x52, 0x45, 0x53, 
0x01, 0x02, 0x58, 0x46, 0xFE, 0xFF, 0x25, 0x00, 0x43, 0x54, 0x41, 0x42, 0x1C, 0x00, 0x00, 0x00, 0x5F, 0x00, 0x00, 0x00, 0x01, 0x02, 0x58, 0x46, 
0x01, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x20, 0x5C, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x4C, 0x00, 0x00, 0x00, 0x42, 0x6C, 0x75, 0x72, 0x41, 0x6D, 0x6F, 0x75, 0x6E, 0x74, 0x00, 0xAB, 
0x00, 0x00, 0x03, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x74, 0x78, 0x00, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x28, 0x52, 0x29, 
0x20, 0x48, 0x4C, 0x53, 0x4C, 0x20, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x20, 0x43, 0x6F, 0x6D, 0x70, 0x69, 0x6C, 0x65, 0x72, 0x20, 0x39, 0x2E, 
0x32, 0x36, 0x2E, 0x39, 0x35, 0x32, 0x2E, 0x32, 0x38, 0x34, 0x34, 0x00, 0xFE, 0xFF, 0x0C, 0x00, 0x50, 0x52, 0x53, 0x49, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x2A, 0x00, 0x43, 0x4C, 0x49, 0x54, 
0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x26, 0xB4, 0x9B, 0xCF, 0x6F, 0x4E, 0x7B, 0x3F, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0xF0, 0x3F, 0x26, 0xB4, 0x9B, 0xCF, 0x6F, 0x4E, 0x8B, 0x3F, 0x1C, 0xC7, 0xB4, 0xDB, 0xD3, 0x7A, 0x94, 0x3F, 0x26, 0xB4, 0x9B, 0xCF, 
0x6F, 0x4E, 0x9B, 0x3F, 0x98, 0x50, 0xC1, 0xE1, 0x05, 0x11, 0xA1, 0x3F, 0x1C, 0xC7, 0xB4, 0xDB, 0xD3, 0x7A, 0xA4, 0x3F, 0xA1, 0x3D, 0xA8, 0xD5, 
0xA1, 0xE4, 0xA7, 0x3F, 0x26, 0xB4, 0x9B, 0xCF, 0x6F, 0x4E, 0xAB, 0x3F, 0xAB, 0x2A, 0x8F, 0xC9, 0x3D, 0xB8, 0xAE, 0x3F, 0x98, 0x50, 0xC1, 0xE1, 
0x05, 0x11, 0xB1, 0x3F, 0xDA, 0x0B, 0xBB, 0xDE, 0xEC, 0xC5, 0xB2, 0x3F, 0x1C, 0xC7, 0xB4, 0xDB, 0xD3, 0x7A, 0xB4, 0x3F, 0x5F, 0x82, 0xAE, 0xD8, 
0xBA, 0x2F, 0xB6, 0x3F, 0xA1, 0x3D, 0xA8, 0xD5, 0xA1, 0xE4, 0xB7, 0x3F, 0xE4, 0xF8, 0xA1, 0xD2, 0x88, 0x99, 0xB9, 0x3F, 0xFE, 0xFF, 0x4E, 0x01, 
0x46, 0x58, 0x4C, 0x43, 0x1E, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 
0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x04, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 
0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x04, 0x00, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 
0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0D, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x04, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 
0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x04, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 
0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 
0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x34, 0x00, 0x00, 0x00, 0x01, 0x00, 0x50, 0xA0, 0x02, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x40, 0xA0, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x04, 0x00, 0x00, 0x00, 0x38, 0x00, 0x00, 0x00, 0xF0, 0xF0, 0xF0, 0xF0, 0x0F, 0x0F, 0x0F, 0x0F, 0xFF, 0xFF, 0x00, 0x00, 0x51, 0x00, 0x00, 0x05, 
0x10, 0x00, 0x0F, 0xA0, 0x00, 0x00, 0x80, 0x3D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x02, 
0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x03, 0xB0, 0x1F, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x90, 0x00, 0x08, 0x0F, 0xA0, 0x42, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0xB0, 0x00, 0x08, 0xE4, 0xA0, 0x02, 0x00, 0x00, 0x03, 0x01, 0x00, 0x03, 0x80, 0x00, 0x00, 0xE4, 0xB0, 
0x0F, 0x00, 0xE4, 0xA1, 0x01, 0x00, 0x00, 0x02, 0x02, 0x00, 0x03, 0x80, 0x0F, 0x00, 0xE4, 0xA0, 0x04, 0x00, 0x00, 0x04, 0x03, 0x00, 0x03, 0x80, 
0x01, 0x00, 0xE4, 0x80, 0x00, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x04, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 
0x01, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x05, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0xA0, 
0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x06, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x03, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 
0x04, 0x00, 0x00, 0x04, 0x07, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 
0x08, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x05, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x09, 0x00, 0x03, 0x80, 
0x01, 0x00, 0xE4, 0x80, 0x06, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x42, 0x00, 0x00, 0x03, 0x03, 0x00, 0x0F, 0x80, 0x03, 0x00, 0xE4, 0x80, 
0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x04, 0x00, 0x0F, 0x80, 0x04, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 
0x05, 0x00, 0x0F, 0x80, 0x05, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x06, 0x00, 0x0F, 0x80, 0x06, 0x00, 0xE4, 0x80, 
0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x07, 0x00, 0x0F, 0x80, 0x07, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 
0x08, 0x00, 0x0F, 0x80, 0x08, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x09, 0x00, 0x0F, 0x80, 0x09, 0x00, 0xE4, 0x80, 
0x00, 0x08, 0xE4, 0xA0, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x03, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x0F, 0x80, 0x04, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x05, 0x00, 0xE4, 0x80, 
0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x06, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x0F, 0x80, 0x07, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x08, 0x00, 0xE4, 0x80, 
0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x09, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 
0x03, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x07, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x04, 0x00, 0x03, 0x80, 
0x01, 0x00, 0xE4, 0x80, 0x08, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x05, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 
0x09, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x06, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x0A, 0x00, 0x00, 0xA0, 
0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x07, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x0B, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 
0x04, 0x00, 0x00, 0x04, 0x08, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x0C, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 
0x09, 0x00, 0x03, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x0D, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x04, 0x00, 0x00, 0x04, 0x01, 0x00, 0x03, 0x80, 
0x01, 0x00, 0xE4, 0x80, 0x0E, 0x00, 0x00, 0xA0, 0x02, 0x00, 0xE4, 0x80, 0x42, 0x00, 0x00, 0x03, 0x02, 0x00, 0x0F, 0x80, 0x03, 0x00, 0xE4, 0x80, 
0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x03, 0x00, 0x0F, 0x80, 0x04, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 
0x04, 0x00, 0x0F, 0x80, 0x05, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x05, 0x00, 0x0F, 0x80, 0x06, 0x00, 0xE4, 0x80, 
0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x06, 0x00, 0x0F, 0x80, 0x07, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 
0x07, 0x00, 0x0F, 0x80, 0x08, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x01, 0x00, 0x0F, 0x80, 0x01, 0x00, 0xE4, 0x80, 
0x00, 0x08, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x08, 0x00, 0x0F, 0x80, 0x09, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x02, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x03, 0x00, 0xE4, 0x80, 
0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x04, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x0F, 0x80, 0x05, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x06, 0x00, 0xE4, 0x80, 
0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x07, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x0F, 0x80, 0x08, 0x00, 0xE4, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x01, 0x00, 0xE4, 0x80, 
0x00, 0x00, 0xE4, 0x80, 0x05, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x10, 0x00, 0x00, 0xA0, 0x01, 0x00, 0x00, 0x02, 
0x00, 0x08, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0xFF, 0xFF, 0x00, 0x00,   
            };
#endif
        }
        #endregion
    }
}
