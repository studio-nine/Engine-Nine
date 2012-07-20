namespace Nine.Graphics.ObjectModel
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public partial class AmbientLight : Light<IAmbientLight>, ISceneObject
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Keeps track of the owner drawing context.
        /// </summary>
        private DrawingContext context;
        
        public AmbientLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            AmbientLightColor = Vector3.One * 0.2f;
        }

        protected override void Enable(IAmbientLight light)
        {
            light.AmbientLightColor = AmbientLightColor;
        }

        protected override void Disable(IAmbientLight light)
        {
            light.AmbientLightColor = Vector3.Zero;
        }

        [ContentSerializer(Optional = true)]
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set
            {
                if (context != null)
                {
                    // Updates ambient light color in the owner context
                    context.AmbientLight.Value += (value - ambientLightColor);
                }
                ambientLightColor = value; 
            }
        }
        private Vector3 ambientLightColor;


        void ISceneObject.OnAdded(DrawingContext context)
        {
            if (this.context != null)
                throw new InvalidOperationException();

            this.context = context;
            this.context.AmbientLight.Value = context.AmbientLight.Value + ambientLightColor;
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {
            if (this.context == null)
                throw new InvalidOperationException();

            this.context.AmbientLight.Value = context.AmbientLight.Value + ambientLightColor;
            this.context = null;
        }
    }
}