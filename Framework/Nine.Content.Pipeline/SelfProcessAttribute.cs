namespace Nine.Content.Pipeline
{
    using System;

    /// <summary>
    /// Specifies the processing method for the object that has self processor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SelfProcessAttribute : Attribute { }
}
