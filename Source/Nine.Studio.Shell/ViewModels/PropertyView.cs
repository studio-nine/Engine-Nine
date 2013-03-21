namespace Nine.Studio.Shell.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    /// <summary>
    /// Adopted from WPF Property Grid (http://wpg.codeplex.com)
    /// </summary>
    public class PropertyView : INotifyPropertyChanged
    {
        public PropertyDescriptor PropertyDescriptor { get; private set; }
        public object Instance { get; private set; }
        public string DisplayName { get { return PropertyDescriptor.DisplayName; } }
        public string Description { get { return PropertyDescriptor.Description; } }
        public string Category { get { return PropertyDescriptor.Category; } }
        public bool IsWriteable { get { return !IsReadOnly; } }
        public bool IsReadOnly { get { return PropertyDescriptor.IsReadOnly; } }
        public Type PropertyType { get { return PropertyDescriptor.PropertyType; } }
        public ICommand SetValueCommand { get; private set; }

        public object Value 
        {
            get { return PropertyDescriptor.GetValue(Instance); }
            set
            {
                object currentValue = PropertyDescriptor.GetValue(Instance);
                if (value != null && value.Equals(currentValue))
                {
                    return;
                }
                Type propertyType = PropertyDescriptor.PropertyType;
                if (propertyType == typeof(object) ||
                    value == null && propertyType.IsClass ||
                    value != null && propertyType.IsAssignableFrom(value.GetType()))
                {
                    PropertyDescriptor.SetValue(Instance, value);
                }
                else
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(PropertyDescriptor.PropertyType);
                    try
                    {
                        object convertedValue = converter.ConvertFrom(value);
                        PropertyDescriptor.SetValue(Instance, convertedValue);
                    }
                    catch (Exception)
                    { }
                }
                NotifyPropertyChanged("Value");
            }
        }

        public PropertyView(object instance, PropertyDescriptor property)
        {
            Instance = instance;
            PropertyDescriptor = property;
            SetValueCommand = new DelegateCommand<object>(SetValue);
        }

        public object GetValue()
        {
            return Value;
        }

        public void SetValue(object value)
        {
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
