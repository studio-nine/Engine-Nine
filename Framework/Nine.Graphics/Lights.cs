namespace Nine.Graphics
{
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Defines an interface for ambient lights
    /// </summary>
    public interface IAmbientLight
    {
        /// <summary>
        /// Gets or sets the ambient light color.
        /// </summary>
        Vector3 AmbientLightColor { get; set; }
    }

    /// <summary>
    /// Defines an interface for directional lights
    /// </summary>
    public interface IDirectionalLight
    {
        /// <summary>
        /// Gets or sets the normalized direction of this light.
        /// </summary>
        Vector3 Direction { get; set; }

        /// <summary>
        /// Gets or sets the diffuse color of this light.
        /// </summary>
        Vector3 DiffuseColor { get; set; }

        /// <summary>
        /// Gets or sets the specular color of this light.
        /// </summary>
        Vector3 SpecularColor { get; set; }
    }

    /// <summary>
    /// Defines an interface for point lights
    /// </summary>
    public interface IPointLight
    {
        /// <summary>
        /// Gets or sets the position of this light.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the diffuse color of this light.
        /// </summary>
        Vector3 DiffuseColor { get; set; }

        /// <summary>
        /// Gets or sets the specular color of this light.
        /// </summary>
        Vector3 SpecularColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the max distance to be lit by this 
        /// light.
        /// </summary>
        float Range { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how quickly the light should falloff
        /// radically.
        /// </summary>
        float Attenuation { get; set; }
    }

    /// <summary>
    /// Defines an interface for spot lights
    /// </summary>
    public interface ISpotLight
    {
        /// <summary>
        /// Gets or sets the position of this light.
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the normalized direction of this light.
        /// </summary>
        Vector3 Direction { get; set; }

        /// <summary>
        /// Gets or sets the diffuse color of this light.
        /// </summary>
        Vector3 DiffuseColor { get; set; }

        /// <summary>
        /// Gets or sets the specular color of this light.
        /// </summary>
        Vector3 SpecularColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the max distance to be lit by this
        /// light.
        /// </summary>
        float Range { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how quickly the light should falloff
        /// radically.
        /// </summary>
        float Attenuation { get; set; }

        /// <summary>
        /// Gets or sets the angle of the inner cone in radius.
        /// </summary>
        float InnerAngle { get; set; }

        /// <summary>
        /// Gets or sets the angle of the outer cone in radius.
        /// </summary>
        float OuterAngle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how quickly the light should falloff 
        /// from the inner cone to the outer cone.
        /// </summary>
        float Falloff { get; set; }
    }
}