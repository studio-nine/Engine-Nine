#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives

#endregion

namespace Nine.Graphics.Effects
{
    /// <summary>
    /// Defines a list of effect parameter semantics supported by the rendering system.
    /// </summary>
    /// <remarks>
    /// Prefix the enum value with "Sas", "SasEffect" or "SasUi" is also supported.
    /// </remarks>
    enum EffectAnnotations
    {
        BindAddress,
        ResourceName,
        Function,
        Dimensions,
    }
}