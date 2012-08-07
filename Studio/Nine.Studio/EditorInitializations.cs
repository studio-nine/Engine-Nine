namespace Nine.Studio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Composition;

    class EditorInitializations : IDisposable
    {
        [ImportMany()]
        public IEnumerable<ISupportInitialize> Initializers { get; set; }

        public EditorInitializations()
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
