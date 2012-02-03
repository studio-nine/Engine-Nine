#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;

#endregion

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// Specifies the processing method for the object that has self processor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SelfProcessAttribute : Attribute { }
}
