// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    /// <summary>
    /// Read SilverlightEffect.
    /// </summary>
    class SilverlightEffectReader : ContentTypeReader<SilverlightEffect>
    {
        /// <summary>
        /// Read and create a SilverlightEffect
        /// </summary>
        protected override SilverlightEffect Read(ContentReader input, SilverlightEffect existingInstance)
        {
            return new SilverlightEffect(input);
        }
    }
}
