namespace Nine
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// Represents a property access expression.
    /// </summary>
    /// <example>
    /// "Name"                  -> Target.Name
    /// "Name.FirstName"        -> Target.Name.FirstName
    /// "Names[0].FirstName"    -> Target.Names[0].FirstName
    /// "Names["n"].FirstName"  -> Target.Names["n"].FirstName
    /// </example>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PropertyExpression<T>
    {
        MemberInfo invocationMember;
        object invocationTarget;

        Action<T> setValue;
        Func<T> getValue;
        
        /// <summary>
        /// Gets or sets the value of the target evaluated using this expression.
        /// </summary>
        public T Value
        {
            get 
            {
                if (getValue != null)
                    return getValue();
                return (T)GetValue(invocationTarget, invocationMember); 
            }
            set 
            {
                if (setValue != null)
                    setValue(value);
                else
                    SetValue(invocationTarget, invocationMember, value); 
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpression&lt;T&gt;"/> class.
        /// </summary>
        public PropertyExpression(object target, string property)
        {
#if WINRT
            throw new NotImplementedException();
#else
            Parse(target, property, out invocationTarget, out invocationMember);

            // Getting a value by reflection without garbage
            //
            // http://stackoverflow.com/questions/2378195/getting-a-value-by-reflection-without-garbage

            if (invocationMember is PropertyInfo)
            {
                try
                {
                    setValue = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), invocationTarget, ((PropertyInfo)invocationMember).GetSetMethod());
                }
                catch { }

                try
                {
                    getValue = (Func<T>)Delegate.CreateDelegate(typeof(Func<T>), invocationTarget, ((PropertyInfo)invocationMember).GetGetMethod());
                }
                catch { }
            }         
#endif
        }

        private static void Parse(object target, string property, out object invocationTarget, out MemberInfo invocationMember)
        {
#if WINRT
            throw new NotImplementedException();
#else
            if (target == null)
                throw new ArgumentNullException("target");
            if (property == null)
                throw new ArgumentNullException("property");

            invocationMember = null;

            var targetType = target.GetType();
            var dot = property.IndexOf('.');
            var currentProperty = dot >= 0 ? property.Substring(0, dot).Trim() : property;
            
            var leftBracket = currentProperty.IndexOf('[');
            var rightBracket = currentProperty.IndexOf(']');
            if (leftBracket * rightBracket < 0)
            {
                throw new ArgumentException("Invalid expression format: " + currentProperty);
            }
            if (leftBracket >= 0)
            {
                if (dot < 0)
                    throw new NotSupportedException();

                var content = currentProperty.Substring(leftBracket + 1, rightBracket - leftBracket - 1).Trim();
                currentProperty = currentProperty.Substring(0, leftBracket);

                if (string.IsNullOrEmpty(currentProperty))
                    invocationTarget = target;
                else
                    invocationTarget = GetValue(target, GetMember(targetType, currentProperty));

                if ((content.StartsWith("\"") && content.EndsWith("\"")) ||
                    (content.StartsWith("'") && content.EndsWith("'")))
                {
                    // String dictionary
                    invocationMember = invocationTarget.GetType().GetProperty("Item", null, new[] { typeof(string) });
                    invocationTarget = GetValue(invocationTarget, invocationMember, content.Substring(1, content.Length - 2));
                }
                else
                {
                    // List
                    invocationMember = invocationTarget.GetType().GetProperty("Item", null, new[] { typeof(int) });
                    invocationTarget = GetValue(invocationTarget, invocationMember, Convert.ToInt32(content));
                }
            }
            else
            {
                invocationTarget = target;
                invocationMember = GetMember(targetType, currentProperty);

                var hasItem = false;
                if (dot >= 0 && invocationMember == null)
                {
                    // String dictionary
                    var items = invocationTarget.GetType().GetProperty("Item", null, new[] { typeof(string) });
                    if (items != null)
                    {
                        invocationTarget = GetValue(target, items, currentProperty);
                        if (invocationTarget != null)
                            hasItem = true;
                    }
                }

                if (!hasItem)
                {
                    if (invocationMember == null)
                    {
                        throw new ArgumentException(string.Format(
                            "Type {0} does not have a valid public property or field {1}.", targetType.FullName, currentProperty));
                    }

                    if (dot >= 0)
                        invocationTarget = GetValue(target, invocationMember);
                }
            }

            if (dot >= 0)
            {
                Parse(invocationTarget, property.Substring(dot + 1, property.Length - dot - 1).Trim(), out invocationTarget, out invocationMember);
            }
#endif
        }

        private static MemberInfo GetMember(Type targetType, string property)
        {
#if WINRT
            throw new NotImplementedException();
#else
            return (MemberInfo)targetType.GetProperty(property) ?? targetType.GetField(property);
#endif
        }

        private static object GetValue(object target, MemberInfo member)
        {
            return (member is FieldInfo) ? ((FieldInfo)(member)).GetValue(target) : ((PropertyInfo)(member)).GetValue(target, null);
        }

        private static object GetValue(object target, MemberInfo member, int index)
        {
            return ((PropertyInfo)(member)).GetValue(target, new object[] { index });
        }

        private static object GetValue(object target, MemberInfo member, string key)
        {
            return ((PropertyInfo)(member)).GetValue(target, new object[] { key });
        }

        private static void SetValue(object target, MemberInfo member, object value)
        {
            if (member is FieldInfo)
                ((FieldInfo)(member)).SetValue(target, value);
            else
                ((PropertyInfo)(member)).SetValue(target, value, null);
        }
    }
}