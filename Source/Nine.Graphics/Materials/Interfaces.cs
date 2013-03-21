namespace Nine.Graphics.Materials
{
    using System.Collections.ObjectModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines an interface for effects that supports shadow mapping.
    /// </summary>
    interface IEffectShadowMap
    {
        /// <summary>
        /// Gets or sets the view projection matrix of the light that casts the shadow.
        /// </summary>
        Matrix LightViewProjection { get; set; }

        /// <summary>
        /// Gets or sets the shadow map texture that holds the depth values in x(r) channel.
        /// </summary>
        Texture2D ShadowMap { get; set; }
    }

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
    interface IEffectLights<T>
    {
        /// <summary>
        /// Gets a read only collection of lights exposed by this effect.
        /// </summary>
        ReadOnlyCollection<T> Lights { get; }
    }
}