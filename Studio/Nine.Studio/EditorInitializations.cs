#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

#endregion

namespace Nine.Studio
{
    class EditorInitializations : IDisposable
    {
        [ImportMany()]
        public IEnumerable<ISupportInitialize> Initializers;

        public EditorInitializations()
        {

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
