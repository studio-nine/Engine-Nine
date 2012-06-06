#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content;
using System.Xaml;
using System;
using System.Reflection;
#endregion

namespace Nine.Content
{
    static class AttachableMemberIdentifierExtensions
    {
        public static void Apply(this AttachableMemberIdentifier identifier, object target, object value)
        {
            if (identifier == null || identifier.DeclaringType == null)
                return;

            var setMethod = identifier.DeclaringType.GetMethod(string.Concat("Set", identifier.MemberName), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);            
            if (setMethod != null)
                setMethod.Invoke(null, new object[] { target, value });
        }
    }
}
