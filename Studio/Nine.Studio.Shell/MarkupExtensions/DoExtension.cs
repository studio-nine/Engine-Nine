namespace Nine.Studio.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Xaml;    

    public class DoExtension : MarkupExtension, ICommand
    {
        private FrameworkElement targetObject;
        private string methodName;
        private bool isInitialized;

        private Func<bool> CanInvoke;
        private Action Invoke0;
        private Action<object> Invoke1;
        
        public DoExtension(string method)
        {
            this.methodName = method;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            targetObject = (FrameworkElement)provideValueTarget.TargetObject;
            return this;
        }

        private void EnsureInitialized()
        {
            if (isInitialized)
                return;

            var modelType = targetObject.DataContext.GetType();
            var viewModelType = Type.GetType(GetType().Namespace + "." + modelType.Name + "View");

            InitializeFromType(modelType);
            if (viewModelType != null)
                InitializeFromType(viewModelType);
            
            isInitialized = true;
        }

        private void InitializeFromType(Type type)
        {
            var method = type.GetMethod(methodName);
            if (method == null)
                return;

            var canInvokeMethod = type.GetMethod("Can" + methodName);
            CanInvoke = canInvokeMethod != null ? (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), canInvokeMethod) : null;

            if (method.GetParameters().Length == 0)
                Invoke0 = (Action)Delegate.CreateDelegate(typeof(Action), method);
            else
                Invoke1 = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), method);
        }
        
        public bool CanExecute(object parameter)
        {
            return CanInvoke == null || CanInvoke();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            EnsureInitialized();

            if (Invoke0 != null)
                Invoke0();
            else
                Invoke1(parameter);
        }
    }
}
