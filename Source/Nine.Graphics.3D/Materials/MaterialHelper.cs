namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    static class MaterialHelper
    {
        /// <summary>
        /// Applies lights to the target effect from the drawing context.
        /// </summary>
        public static void ApplyLights(DrawingContext3D context, IEffectLights effect)
        {
            // Update ambient light color
            effect.AmbientLightColor = context.ambientLightColor;

            var light0 = context.directionalLights[0];
            var light1 = context.directionalLights[1];
            var light2 = context.directionalLights[2];

            // Update the shader light parameters only when it changed.
            if (effect.DirectionalLight0.Enabled = light0.Enabled)
            {
                effect.DirectionalLight0.Direction = light0.Direction;
                effect.DirectionalLight0.DiffuseColor = light0.DiffuseColor;
                effect.DirectionalLight0.SpecularColor = light0.SpecularColor;
            }

            if (effect.DirectionalLight1.Enabled = light1.Enabled)
            {
                effect.DirectionalLight1.Direction = light1.Direction;
                effect.DirectionalLight1.DiffuseColor = light1.DiffuseColor;
                effect.DirectionalLight1.SpecularColor = light1.SpecularColor;
            }

            if (effect.DirectionalLight2.Enabled = light2.Enabled)
            {
                effect.DirectionalLight2.Direction = light2.Direction;
                effect.DirectionalLight2.DiffuseColor = light2.DiffuseColor;
                effect.DirectionalLight2.SpecularColor = light2.SpecularColor;
            }
        }

        /// <summary>
        /// Applies fog to the target effect from the drawing context.
        /// </summary>        
        public static void ApplyFog(DrawingContext3D context, IEffectFog effect)
        {
            effect.FogColor = context.FogColor;
            effect.FogStart = context.FogStart;
            effect.FogEnd = context.FogEnd;
        }
    }
}