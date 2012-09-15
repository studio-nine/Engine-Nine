namespace Nine.Content
{
    using System;
    using Microsoft.Xna.Framework.Content;

    class NullReader : ContentTypeReader
    {
        public NullReader() : base(typeof(NullReader)) { }

        protected override object Read(ContentReader input, object existingInstance)
        {
            return null;
        }
    }
}