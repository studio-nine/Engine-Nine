namespace Nine.Graphics.Materials
{
    using System.Collections.ObjectModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

#if !WINDOWS_PHONE

    /// <summary>
    /// Defines an interface for effects that supports shadow mapping.
    /// </summary>
    public interface IEffectShadowMap
    {
        /// <summary>
        /// Gets or sets a small depth bias value that is added to the shadow map depth
        /// when comparing the object depth with depth in the shadow map.
        /// </summary>
        float DepthBias { get; set; }

        /// <summary>
        /// Gets or sets the color of the shadow.
        /// </summary>
        Vector3 ShadowColor { get; set; }

        /// <summary>
        /// Gets or sets the view projection matrix of the light that casts the shadow.
        /// </summary>
        Matrix LightViewProjection { get; set; }

        /// <summary>
        /// Gets or sets the shadow map texture that holds the depth values in x(r) channel.
        /// </summary>
        Texture2D ShadowMap { get; set; }
    }

#endif

    /// <summary>
    /// Gets or sets skinning parameters for the current effect.
    /// </summary>
    public interface IEffectSkinned
    {
        /// <summary>
        /// Gets or sets if vertex skinning is enabled by this effect.
        /// </summary>
        bool SkinningEnabled { get; set; }

        /// <summary>
        /// Sets the bones transforms for the skinned effect.
        /// </summary>
        void SetBoneTransforms(Matrix[] boneTransforms);
    }

    /// <summary>
    /// Gets or sets lighting parameters for the current effect.
    /// </summary>
    public interface IEffectLights<T>
    {
        /// <summary>
        /// Gets a read only collection of lights exposed by this effect.
        /// </summary>
        ReadOnlyCollection<T> Lights { get; }
    }
}