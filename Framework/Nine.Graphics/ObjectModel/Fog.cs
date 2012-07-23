namespace Nine.Graphics.ObjectModel
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines an area of fog.
    /// </summary>
    [ContentSerializable]
    public class Fog : Object, ISceneObject
    {
        public float Start { get; set; }
        public float End { get; set; }
        public Vector3 Color { get; set; }
        public bool Enabled { get; set; }

        private DrawingContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="Fog"/> class.
        /// </summary>
        public Fog()
        {
            Start = MaterialConstants.FogStart;
            End = MaterialConstants.FogEnd;
            Enabled = MaterialConstants.FogEnabled;
            Color = MaterialConstants.FogColor;
        }

        void ISceneObject.OnAdded(DrawingContext context)
        {
            this.context = context;
            this.context.FogStart = Start;
            this.context.FogEnd = End;
            this.context.FogEnabled = Enabled;
            this.context.FogColor = Color;
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {
            this.context = null;
        }
    }
}