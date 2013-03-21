namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [Nine.Serialization.BinarySerializable]
    public class AmbientLight : Nine.Object, IGraphicsObject
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        private DrawingContext3D context;
        
        public AmbientLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            AmbientLightColor = Vector3.One * 0.2f;
        }

        public bool Enabled
        {
            get { return enabled; }
            set 
            {
                if (enabled != value)
                {
                    if (context != null)
                    {
                        if (value)
                            context.AmbientLightColor += ambientLightColor;
                        else
                            context.AmbientLightColor -= ambientLightColor;
                    }
                    enabled = value; 
                }
            }
        }
        private bool enabled = true;

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set
            {
                if (context != null)
                {
                    // Updates ambient light color in the owner context
                    context.AmbientLightColor += (value - ambientLightColor);
                }
                ambientLightColor = value; 
            }
        }
        private Vector3 ambientLightColor;

        void IGraphicsObject.OnAdded(DrawingContext context)
        {
            this.context = context as DrawingContext3D;
            if (enabled)
                this.context.AmbientLightColor = this.context.AmbientLightColor + ambientLightColor;
        }

        void IGraphicsObject.OnRemoved(DrawingContext context)
        {
            if (enabled)
                this.context.AmbientLightColor = this.context.AmbientLightColor - ambientLightColor;
            this.context = null;
        }
    }
}