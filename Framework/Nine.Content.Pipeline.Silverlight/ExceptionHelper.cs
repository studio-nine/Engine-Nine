// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using Microsoft.Xna.Framework.Content.Pipeline;
using System;

namespace SilverlightContentPipeline
{
    internal static class ExceptionHelper
    {
        public static string Filename { get; set; }

        public static void RaiseException(string message, Exception innerException = null)
        {
            throw new InvalidContentException(message, new ContentIdentity(Filename), innerException);
        }
    }
}
