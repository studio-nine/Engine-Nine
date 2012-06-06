#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Windows.Markup;
using System.Xaml;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    struct PropertyInstance : IEquatable<PropertyInstance>
    {
        public object Target;
        public string TargetProperty;

        public PropertyInstance(object target, string targetProperty)
        {
            Target = target;
            TargetProperty = targetProperty;
        }

        public bool Equals(PropertyInstance other)
        {
            return Target == other.Target && TargetProperty == other.TargetProperty;
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyInstance)
                return Equals((PropertyInstance)obj);
            return false;
        }

        public static bool operator ==(PropertyInstance value1, PropertyInstance value2)
        {
            return ((value1.Target == value2.Target) && (value1.TargetProperty == value2.TargetProperty));
        }

        public static bool operator !=(PropertyInstance value1, PropertyInstance value2)
        {
            return !(value1.Target == value2.Target && value1.TargetProperty == value2.TargetProperty);
        }

        public override int GetHashCode()
        {
            return (Target != null ? Target.GetHashCode() : 0) ^ (TargetProperty != null ? TargetProperty.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return (Target != null ? Target.GetType().Name : "") + "." + (TargetProperty != null ? TargetProperty : "");
        }
    }
}
