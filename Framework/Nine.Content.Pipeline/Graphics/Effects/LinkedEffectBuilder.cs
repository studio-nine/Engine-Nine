#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nine.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Enables a centralized place where LinkedEffect can be compiled and cached.
    /// Use this library to eliminated duplicated LinkedEffects.
    /// </summary>
    public static class LinkedEffectBuilder
    {
        /// <summary>
        /// Builds the specified linked effect.
        /// </summary>
        /// <param name="linkedEffect">The linked effect.</param>
        /// <param name="context">The context.</param>
        /// <returns>The asset name of the target file.</returns>
        public static ExternalReference<LinkedEffectContent> Build(LinkedEffectContent linkedEffect, ContentProcessorContext context)
        {
            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
            {
                context.Logger.LogWarning(null, null, Strings.LinkedEffectNotSupported);
                return new ExternalReference<LinkedEffectContent>(Strings.LinkedEffectNotSupported);
            }

            // Build the source asset
            linkedEffect = new LinkedEffectProcessor().Process(linkedEffect, context);
            var hash = linkedEffect.Token;
            var hashString = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                hashString.Append(hash[i].ToString("X2"));
            }
            return context.BuildAsset<LinkedEffectContent, LinkedEffectContent>(linkedEffect, null, null,
                   Path.Combine(ContentProcessorContextExtensions.DefaultOutputDirectory, hashString.ToString()));
        }
    }
}
