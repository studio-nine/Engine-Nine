namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// A drawing context contains commonly used global parameters for 3D rendering.
    /// </summary>
    public class DrawingContext3D : DrawingContext
    {
        #region Lights
        /// <summary>
        /// Gets the global ambient light color of this <see cref="DrawingContext"/>.
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; }
        }
        internal Vector3 ambientLightColor;

        /// <summary>
        /// Gets a global sorted collection of directional lights of this <see cref="DrawingContext"/>.
        /// </summary>
        public DirectionalLightCollection DirectionalLights
        {
            get { return directionalLights; }
        }
        internal DirectionalLightCollection directionalLights;

        /// <summary>
        /// Gets the default or main directional light of this <see cref="DrawingContext"/>.
        /// </summary>
        public DirectionalLight DirectionalLight
        {
            get { return directionalLights[0] ?? defaultLight; }
        }
        private DirectionalLight defaultLight;
        #endregion

        #region Fog
        /// <summary>
        /// Gets the global fog color.
        /// </summary>
        public Vector3 FogColor
        {
            get { return fogColor; }
            set { fogColor = value; }
        }
        internal Vector3 fogColor = Constants.FogColor;

        /// <summary>
        /// Gets or sets the fog end.
        /// </summary>
        public float FogEnd
        {
            get { return fogEnd; }
            set { fogEnd = value; }
        }
        internal float fogEnd = Constants.FogEnd;

        /// <summary>
        /// Gets or sets the fog start.
        /// </summary>
        public float FogStart
        {
            get { return fogStart; }
            set { fogStart = value; }
        }
        internal float fogStart = Constants.FogStart;
        #endregion
        
        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        public DrawingContext3D(GraphicsDevice graphics) : this(graphics, new SpatialQuery())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        public DrawingContext3D(GraphicsDevice graphics, IEnumerable objects) : this(graphics, new SpatialQuery(objects))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContext"/> class.
        /// </summary>
        public DrawingContext3D(GraphicsDevice graphics, ISpatialQuery spatialQuery) : base(graphics, spatialQuery)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (spatialQuery == null)
                throw new ArgumentNullException("spatialQuery");

            this.defaultLight = new DirectionalLight(graphics)
            {
                DiffuseColor = Vector3.Zero,
                SpecularColor = Vector3.Zero,
                Direction = Vector3.Down,
                Enabled = false
            };
            this.directionalLights = new DirectionalLightCollection(defaultLight);
            
            //this.rootPass.Passes.Add(mainPass = new DrawingPass() { ClearBackground = true, TransparencySortEnabled = true });
            //this.rootPass.Passes.Add(new SpritePass());
        }
        #endregion
    }
}