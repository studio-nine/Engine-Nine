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
            return new SilverlightEffect(GraphicsDeviceManager.Current.GraphicsDevice, input.ReadRawObject<byte[]>());
        }
    }
}
