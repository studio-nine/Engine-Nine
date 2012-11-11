namespace Nine.Studio.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;    

    public class TypeExtension : MarkupExtension
    {
        private System.Windows.Markup.TypeExtension typeExtension;

        public TypeExtension(string type)
        {
            this.typeExtension = new System.Windows.Markup.TypeExtension(type);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object value = null;
            var type = (Type)typeExtension.ProvideValue(serviceProvider);
            if (!Instances.TryGetValue(type, out value))
                Instances[type] = value = Activator.CreateInstance(type);
            return value;
        }

        private static Dictionary<Type, object> Instances = new Dictionary<Type, object>();
    }
}
