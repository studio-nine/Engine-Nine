namespace Nine.Serialization
{
    using Microsoft.Xna.Framework.Content;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class XnbReader : IBinaryObjectReader
    {
        //XnbLoader loader;

        public object Read(BinaryReader input, object existingInstance, IServiceProvider serviceProvider)
        {
            /*
            if (loader == null)
                loader = new XnbLoader(serviceProvider);
            return loader.Load<object>(input.BaseStream);
             */
            return null;
        }
    }
}