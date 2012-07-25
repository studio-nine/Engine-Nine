namespace Nine.Graphics.ObjectModel
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Defines an area of fog.
    /// </summary>
    [ContentSerializable]
    public class Fog : Object, ISceneObject
    {
        public float FogStart { get; set; }
        public float FogEnd { get; set; }
        public Vector3 FogColor { get; set; }
        public bool Enabled { get; set; }

        private DrawingContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fog"/> class.
        /// </summary>
        public Fog()
        {
            FogStart = MaterialConstants.FogStart;
            FogEnd = MaterialConstants.FogEnd;
            Enabled = MaterialConstants.FogEnabled;
            FogColor = MaterialConstants.FogColor;
        }

        void ISceneObject.OnAdded(DrawingContext context)
        {
            this.context = context;
            this.context.FogStart = FogStart;
            this.context.FogEnd = FogEnd;
            this.context.FogEnabled = Enabled;
            this.context.FogColor = FogColor;
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {
            this.context = null;
        }
    }
}