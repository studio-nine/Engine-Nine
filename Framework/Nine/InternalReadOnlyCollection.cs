#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Nine
{
    /// <summary>
    /// A ReadOnlyCollection that can be accessed internally.
    /// </summary>
    internal class InternalReadOnlyCollection<T> : ReadOnlyCollection<T>
    {
        public InternalReadOnlyCollection() : base(new List<T>()) { }
        public InternalReadOnlyCollection(IList<T> list) : base(list) { }

        internal IList<T> InternalItems { get { return Items; } }
    }
}