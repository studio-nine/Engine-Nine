namespace Nine.Studio.Extensibility
{
    using System;

    /// <summary>
    /// Represents a design tool that edit an object
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Gets the target type that can be edited by this IDesigner.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Edits the object.
        /// </summary>
        void Edit(Editor editor, object value);
    }

    /// <summary>
    /// Generic base class implementing ITool
    /// </summary>
    public abstract class Tool<T> : ITool
    {
        public Editor Editor { get; private set; }
        public Type TargetType { get { return typeof(T); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tool&lt;T&gt;"/> class.
        /// </summary>
        public Tool()
        {

        }

        void ITool.Edit(Editor editor, object value)
        {
            Verify.IsNotNull(editor, "editor");
            Verify.IsAssignableFrom(value, typeof(T), "value");

            Editor = editor;
            Edit((T)value);
        }

        /// <summary>
        /// Edits the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        protected abstract void Edit(T value);
    }
}
