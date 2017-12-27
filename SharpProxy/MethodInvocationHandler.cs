using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpProxy
{
    public delegate object MethodInvocationHandler(object target, MethodInvocationHandlerEventArgs e);

    public class MethodInvocationHandlerEventArgs : EventArgs
    {
        private MethodInfo calledMethod;
        private Dictionary<string, object> parameters;

        public MethodInvocationHandlerEventArgs(MethodInfo calledMethod, Dictionary<string, object> parameters)
        {
            this.calledMethod = calledMethod;
            this.parameters = parameters;
        }

        public MethodInfo CalledMethod
        {
            get
            {
                return calledMethod;
            }
        }

        public Dictionary<string, object> Parameters
        {
            get
            {
                return parameters;
            }
        }
    }
}
