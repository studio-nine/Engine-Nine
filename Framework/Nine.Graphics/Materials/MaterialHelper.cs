namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    struct MaterialLightHelper
    {
        private int lightVersion0;
        private int lightVersion1;
        private int lightVersion2;
        private int lightCollectionVersion;
        private int ambientLightVersion;

        public void Apply(DrawingContext context, IEffectLights effect)
        {
            // Update ambient light color
            if (ambientLightVersion != context.ambientLightColorVersion)
            {
                effect.AmbientLightColor = context.ambientLightColor;
                ambientLightVersion = context.ambientLightColorVersion;
            }

            var light0 = context.DirectionalLights[0];
            var light1 = context.DirectionalLights[1];
            var light2 = context.DirectionalLights[2];

            // Force update of the light when the lights are added, replaced or removed.
            var force = false;
            var contextLightCollectionVersion = context.DirectionalLights.Version;
            if (lightCollectionVersion != contextLightCollectionVersion)
            {
                lightCollectionVersion = contextLightCollectionVersion;
                force = true;
            }

            if (light0 == null)
            {
                effect.DirectionalLight0.Enabled = false;
            }
            else if (light0.Version != lightVersion0 || force)
            {
                // Update the shader light parameters only when it changed.
                effect.DirectionalLight0.Direction = light0.Direction;
                effect.DirectionalLight0.DiffuseColor = light0.DiffuseColor;
                effect.DirectionalLight0.SpecularColor = light0.SpecularColor;
                effect.DirectionalLight0.Enabled = light0.Enabled;
                lightVersion0 = light0.Version;
            }

            if (light1 == null)
            {
                effect.DirectionalLight1.Enabled = false;
            }
            else if (light1.Version != lightVersion1 || force)
            {
                effect.DirectionalLight1.Direction = light1.Direction;
                effect.DirectionalLight1.DiffuseColor = light1.DiffuseColor;
                effect.DirectionalLight1.SpecularColor = light1.SpecularColor;
                effect.DirectionalLight1.Enabled = light1.Enabled;
                lightVersion1 = light1.Version;
            }

            if (light2 == null)
            {
                effect.DirectionalLight2.Enabled = false;
            }
            else if (light2.Version != lightVersion2 || force)
            {
                effect.DirectionalLight2.Direction = light2.Direction;
                effect.DirectionalLight2.DiffuseColor = light2.DiffuseColor;
                effect.DirectionalLight2.SpecularColor = light2.SpecularColor;
                effect.DirectionalLight2.Enabled = light2.Enabled;
                lightVersion2 = light2.Version;
            }
        }
    }

    struct MaterialFogHelper
    {
        private int fogVersion;

        public void Apply(DrawingContext context, IEffectFog effect)
        {
            if (context.fogVersion != fogVersion)
            {
                effect.FogColor = context.FogColor;
                effect.FogStart = context.FogStart;
                effect.FogEnd = context.FogEnd;
                effect.FogEnabled = context.FogEnabled;

                fogVersion = context.fogVersion;
            }
        }
    }
}