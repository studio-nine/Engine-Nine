namespace Nine.Graphics.ObjectModel
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    class FogProperty : IEffectFog
    {
        private Versioned<IEffectFog> fog;

        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; fog.version++; }
        }
        private Vector3 fogColor = MaterialConstants.FogColor;

        public bool FogEnabled
        {
            get { return fogEnabled; }
            set { if (fogEnabled != value) { fogEnabled = value; fog.version++; } }
        }
        private bool fogEnabled = MaterialConstants.FogEnabled;

        public float FogEnd
        {
            get { return fogEnd; }
            set { fogEnd = value; fog.version++; }
        }
        private float fogEnd = MaterialConstants.FogEnd;

        public float FogStart
        {
            get { return fogStart; }
            set { fogStart = value; fog.version++; }
        }
        private float fogStart = MaterialConstants.FogStart;

        public FogProperty(Versioned<IEffectFog> fog)
        {
            this.fog = fog;
            this.FogEnabled = MaterialConstants.FogEnabled;
            this.FogStart = MaterialConstants.FogStart;
            this.FogEnd = MaterialConstants.FogEnd;
            this.FogColor = MaterialConstants.FogColor;
        }
    }

    /// <summary>
    /// Defines an area of fog.
    /// </summary>
    [ContentSerializable]
    public class Fog : Transformable, IEffectFog
    {
        public float FogStart { get; set; }
        public float FogEnd { get; set; }
        public Vector3 FogColor { get; set; }
        public bool FogEnabled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Fog"/> class.
        /// </summary>
        public Fog()
        {
            FogStart = 1000;
            FogEnd = 10000;
            FogEnabled = true;
            FogColor = Vector3.One;
        }
    }
}