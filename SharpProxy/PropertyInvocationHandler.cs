using System;
using System.Reflection;

namespace SharpProxy
{
    public delegate object PropertyInvocationHandler(object target, PropertyInvocationHandlerEventArgs e);

    public class PropertyInvocationHandlerEventArgs : EventArgs
    {
        private PropertyInfo property;
        private object value;
        private bool isUpdating;

        public PropertyInvocationHandlerEventArgs(PropertyInfo property, object value, bool isUpdating)
        {
            this.property = property;
            this.value = value;
            this.isUpdating = isUpdating;
        }

        public PropertyInfo InvokedProperty
        {
            get
            {
                return property;
            }
        }

        public object Value
        {
            get
            {
                return value;
            }
        }

        public bool IsUpdating
        {
            get
            {
                return isUpdating;
            }
        }
    }
}
