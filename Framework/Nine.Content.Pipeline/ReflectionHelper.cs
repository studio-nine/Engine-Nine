#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Reflection;
#endregion

namespace Nine.Content.Pipeline
{
    static class ReflectionHelper
    {
        public static T CreateInstance<T>(params object[] args)
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T), args);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public static object Invoke(object target, string method, params object[] args)
        {
            try
            {
                MethodInfo methodInfo = target.GetType().GetMethod(method,
                                                   BindingFlags.Instance | BindingFlags.InvokeMethod |
                                                   BindingFlags.NonPublic | BindingFlags.Public, null,
                                                   args.Select(o => o.GetType()).ToArray(), null);

                return methodInfo.Invoke(target, args);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
