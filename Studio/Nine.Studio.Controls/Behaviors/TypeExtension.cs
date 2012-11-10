namespace Nine.Studio.Shell.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;    

    public class TypeExtension : MarkupExtension
    {
        private System.Windows.Markup.TypeExtension typeExtension;
        private object value;

        public TypeExtension(string type)
        {
            this.typeExtension = new System.Windows.Markup.TypeExtension(type);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var type = (Type)typeExtension.ProvideValue(serviceProvider);
            if (!Converters.TryGetValue(type, out value))
                Converters[type] = value = Activator.CreateInstance(type);
            return value;
        }

        private static Dictionary<Type, object> Converters = new Dictionary<Type, object>();
    }
}
