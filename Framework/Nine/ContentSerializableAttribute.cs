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

namespace Nine
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    class ContentSerializableAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    class NotContentSerializableAttribute : Attribute { }
}