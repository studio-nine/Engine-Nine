// -----------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by EffectCustomTool v1.3.0.0.
//     Runtime Version: v4.0.30319
//
//     EffectCustomTool is a part of Engine Nine. (http://nine.codeplex.com)
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// -----------------------------------------------------------------------------

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

	using Microsoft.Xna.Framework;
	using Microsoft.Xna.Framework.Graphics;

    partial class EmbossEffect : Effect
    {		
        public EmbossEffect(GraphicsDevice graphics) : base(graphics, effectCode)
        {
            CacheEffectParameters(this);

			OnCreated();
        }

        /// <summary>
        /// Creates a new EmbossEffect by cloning parameter settings from an existing instance.
        /// </summary>
		protected EmbossEffect(EmbossEffect cloneSource) : base(cloneSource)
		{
            CacheEffectParameters(cloneSource);

			OnCreated();

            this._pixelSize = cloneSource._pixelSize;
            this._Emboss = cloneSource._Emboss;

			
			OnClone(cloneSource);
		}

        /// <summary>
        /// Creates a clone of the current EmbossEffect instance.
        /// </summary>
        public override Effect Clone()
        {
            return new EmbossEffect(this);
        }

        private void CacheEffectParameters(Effect cloneSource)
        {
            this._pixelSizeParameter = cloneSource.Parameters["pixelSize"];
            this._EmbossParameter = cloneSource.Parameters["Emboss"];

        }

		#region Dirty Flags

		uint dirtyFlag = 0;

        const uint pixelSizeDirtyFlag = 1 << 0;
        const uint EmbossDirtyFlag = 1 << 1;

		#endregion

		#region Properties

        private Vector2 _pixelSize;
        private EffectParameter _pixelSizeParameter;

        internal Vector2 pixelSize
        {
            get { return _pixelSize; }
            set { _pixelSize = value; dirtyFlag |= pixelSizeDirtyFlag; }
        }

        private float _Emboss;
        private EffectParameter _EmbossParameter;

        public float Emboss
        {
            get { return _Emboss; }
            set { _Emboss = value; dirtyFlag |= EmbossDirtyFlag; }
        }


		#endregion
		
		#region Apply
        protected override void OnApply()
        {
			OnApplyChanges();

            if ((this.dirtyFlag & pixelSizeDirtyFlag) != 0)
            {
                this._pixelSizeParameter.SetValue(_pixelSize);
                this.dirtyFlag &= ~pixelSizeDirtyFlag;
            }
            if ((this.dirtyFlag & EmbossDirtyFlag) != 0)
            {
                this._EmbossParameter.SetValue(_Emboss);
                this.dirtyFlag &= ~EmbossDirtyFlag;
            }

            base.OnApply();
        }
		#endregion

        #region ByteCode
        static byte[] effectCode = null;

        static EmbossEffect()
        {
#if XBOX360
            effectCode = new byte[] 
            {
0xBC, 0xF0, 0x0B, 0xCF, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0x09, 0x01, 0x00, 0x00, 0x00, 0xC0, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0D, 0x73, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x53, 0x74, 0x61, 0x74, 0x65, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x70, 0x69, 0x78, 0x65, 0x6C, 0x53, 0x69, 0x7A, 
0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x45, 0x6D, 0x62, 0x6F, 0x73, 0x73, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x03, 0x70, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x50, 0x6F, 0x73, 0x74, 0x50, 0x72, 0x6F, 0x63, 0x65, 0x73, 0x73, 0x00, 
0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x18, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00, 0x00, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x64, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB0, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xA8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x5D, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x48, 0x10, 0x2A, 0x11, 0x00, 0x00, 0x00, 0x01, 0x54, 
0x00, 0x00, 0x00, 0xF4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x01, 0x08, 0x00, 0x00, 0x01, 0x30, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE0, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0xD3, 0xFF, 0xFF, 0x03, 0x00, 0x00, 0x00, 0x00, 0x03, 
0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCC, 0x00, 0x00, 0x00, 0x58, 0x00, 0x02, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00, 0x70, 0x00, 0x00, 0x00, 0x80, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x8C, 
0x00, 0x00, 0x00, 0x9C, 0x00, 0x00, 0x00, 0xAC, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBC, 0x00, 0x00, 0x00, 0x00, 
0x45, 0x6D, 0x62, 0x6F, 0x73, 0x73, 0x00, 0xAB, 0x00, 0x00, 0x00, 0x03, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x3F, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x69, 0x78, 0x65, 0x6C, 0x53, 0x69, 0x7A, 
0x65, 0x00, 0xAB, 0xAB, 0x00, 0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x53, 0x74, 0x61, 0x74, 0x65, 
0x00, 0xAB, 0xAB, 0xAB, 0x00, 0x04, 0x00, 0x0C, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x73, 0x5F, 0x33, 
0x5F, 0x30, 0x00, 0x32, 0x2E, 0x30, 0x2E, 0x31, 0x31, 0x36, 0x32, 0x36, 0x2E, 0x30, 0x00, 0xAB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x01, 0xFC, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0xB4, 0x10, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x04, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x21, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x30, 0x50, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 
0x3E, 0xAA, 0xAA, 0xAB, 0x3F, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x40, 0x02, 0x00, 0x00, 0x12, 0x00, 0xC4, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x60, 0x06, 0x20, 0x0C, 0x12, 0x00, 0x22, 0x00, 0x00, 0x00, 0xC8, 0x0C, 0x00, 0x00, 0x02, 0xAC, 0xAC, 0x00, 0xA0, 0x00, 0x00, 0x00, 
0xC8, 0x03, 0x00, 0x00, 0x00, 0xB0, 0xB0, 0x00, 0xA0, 0x00, 0x00, 0x00, 0x10, 0x08, 0x10, 0x01, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 
0xB8, 0x08, 0x20, 0x01, 0x1F, 0x1F, 0xF6, 0x88, 0x00, 0x00, 0x40, 0x00, 0xC8, 0x03, 0x00, 0x00, 0x00, 0x1A, 0x6C, 0x00, 0xA1, 0x02, 0x01, 0x00, 
0xB8, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x41, 0xC2, 0x00, 0x00, 0xFF, 0xC8, 0x01, 0x00, 0x00, 0x01, 0xC5, 0x6C, 0x6C, 0xB1, 0x01, 0x01, 0x00, 
0xC8, 0x08, 0x00, 0x00, 0x04, 0xB0, 0x6C, 0x6C, 0xB1, 0x02, 0x01, 0x00, 0xC8, 0x06, 0x00, 0x00, 0x00, 0x1C, 0x6C, 0xCB, 0xAB, 0x01, 0x01, 0x00, 
0xB0, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC1, 0xC2, 0x00, 0x00, 0xFF, 0xA8, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0xC2, 0x00, 0x00, 0xFF, 
0xC8, 0x0F, 0x80, 0x00, 0x00, 0xEC, 0xEC, 0x00, 0xE2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  
            };
#else
            effectCode = new byte[] 
            {
0xCF, 0x0B, 0xF0, 0xBC, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x09, 0xFF, 0xFE, 0xC0, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x0D, 0x00, 0x00, 0x00, 0x73, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x53, 0x74, 0x61, 0x74, 0x65, 0x00, 0xD7, 0x16, 0x03, 
0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x54, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x70, 0x69, 0x78, 0x65, 0x6C, 0x53, 0x69, 0x7A, 
0x65, 0x00, 0x16, 0x03, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x07, 0x00, 0x00, 0x00, 0x45, 0x6D, 0x62, 0x6F, 0x73, 0x73, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x03, 0x00, 0x00, 0x00, 0x70, 0x30, 0x00, 0x03, 0x0C, 0x00, 0x00, 0x00, 0x50, 0x6F, 0x73, 0x74, 0x50, 0x72, 0x6F, 0x63, 0x65, 0x73, 0x73, 0x00, 
0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x00, 0x00, 0x00, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x64, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x01, 0x00, 0x00, 0x00, 0xA8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x93, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x94, 0x00, 0x00, 0x00, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0xFE, 0xFF, 0x42, 0x00, 
0x43, 0x54, 0x41, 0x42, 0x1C, 0x00, 0x00, 0x00, 0xD3, 0x00, 0x00, 0x00, 0x00, 0x02, 0xFF, 0xFF, 0x03, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x20, 0xCC, 0x00, 0x00, 0x00, 0x58, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x60, 0x00, 0x00, 0x00, 
0x70, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x8C, 0x00, 0x00, 0x00, 0x9C, 0x00, 0x00, 0x00, 
0xAC, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0xBC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x45, 0x6D, 0x62, 0x6F, 
0x73, 0x73, 0x00, 0xAB, 0x00, 0x00, 0x03, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x69, 0x78, 0x65, 0x6C, 0x53, 0x69, 0x7A, 0x65, 0x00, 0xAB, 0xAB, 
0x01, 0x00, 0x03, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x73, 0x61, 0x6D, 0x70, 0x6C, 0x65, 0x72, 0x53, 0x74, 0x61, 0x74, 0x65, 0x00, 0xAB, 0xAB, 0xAB, 
0x04, 0x00, 0x0C, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0x73, 0x5F, 0x32, 0x5F, 0x30, 0x00, 0x4D, 
0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x20, 0x28, 0x52, 0x29, 0x20, 0x48, 0x4C, 0x53, 0x4C, 0x20, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 
0x20, 0x43, 0x6F, 0x6D, 0x70, 0x69, 0x6C, 0x65, 0x72, 0x20, 0x39, 0x2E, 0x32, 0x36, 0x2E, 0x39, 0x35, 0x32, 0x2E, 0x32, 0x38, 0x34, 0x34, 0x00, 
0x51, 0x00, 0x00, 0x05, 0x02, 0x00, 0x0F, 0xA0, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x80, 0x3F, 
0x51, 0x00, 0x00, 0x05, 0x03, 0x00, 0x0F, 0xA0, 0xAB, 0xAA, 0xAA, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x1F, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x03, 0xB0, 0x1F, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x90, 0x00, 0x08, 0x0F, 0xA0, 
0x02, 0x00, 0x00, 0x03, 0x00, 0x00, 0x03, 0x80, 0x00, 0x00, 0xE4, 0xB0, 0x00, 0x00, 0xE4, 0xA1, 0x02, 0x00, 0x00, 0x03, 0x01, 0x00, 0x03, 0x80, 
0x00, 0x00, 0xE4, 0xB0, 0x00, 0x00, 0xE4, 0xA0, 0x42, 0x00, 0x00, 0x03, 0x00, 0x00, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 
0x42, 0x00, 0x00, 0x03, 0x01, 0x00, 0x0F, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x00, 0x08, 0xE4, 0xA0, 0x01, 0x00, 0x00, 0x02, 0x02, 0x00, 0x08, 0x80, 
0x01, 0x00, 0x00, 0xA0, 0x04, 0x00, 0x00, 0x04, 0x00, 0x00, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0xFF, 0x81, 0x02, 0x00, 0xE4, 0xA0, 
0x04, 0x00, 0x00, 0x04, 0x00, 0x00, 0x0F, 0x80, 0x01, 0x00, 0xE4, 0x80, 0x01, 0x00, 0x00, 0xA0, 0x00, 0x00, 0xE4, 0x80, 0x02, 0x00, 0x00, 0x03, 
0x01, 0x00, 0x01, 0x80, 0x00, 0x00, 0x55, 0x80, 0x00, 0x00, 0x00, 0x80, 0x02, 0x00, 0x00, 0x03, 0x01, 0x00, 0x01, 0x80, 0x00, 0x00, 0xAA, 0x80, 
0x01, 0x00, 0x00, 0x80, 0x05, 0x00, 0x00, 0x03, 0x00, 0x00, 0x07, 0x80, 0x01, 0x00, 0x00, 0x80, 0x03, 0x00, 0x00, 0xA0, 0x01, 0x00, 0x00, 0x02, 
0x00, 0x08, 0x0F, 0x80, 0x00, 0x00, 0xE4, 0x80, 0xFF, 0xFF, 0x00, 0x00,   
            };
#endif
        }
        #endregion
    }

#endif
}