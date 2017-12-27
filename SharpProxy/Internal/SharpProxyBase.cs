using System;
using System.Collections.Generic;
using System.Reflection;

namespace SharpProxy.Internal
{
    public class SharpProxyBase
    {
        private InvocationHandlerManager invocationHandler;
        public SharpProxyBase(InvocationHandlerManager invocationHandler)
        {
            this.invocationHandler = invocationHandler;
        }

        public object ProcessMethodInvocation(MethodInfo callingMethod, object[] args)
        {
            Dictionary<string, object> _args = new Dictionary<string, object>();
            ParameterInfo[] _params = callingMethod.GetParameters();

            for (int i = 0; i < _params.Length && i < args.Length; i++)
            {
                ParameterInfo param = _params[i];

                _args.Add(param.Name, args[i]);
            }

            MethodInvocationHandlerEventArgs e = new MethodInvocationHandlerEventArgs(callingMethod.GetBaseDefinition(), _args);
            return this.invocationHandler.HandleOnMethodInvoked(invocationHandler, e);
        }

        public object ProcessPropertyGetInvocation(PropertyInfo property)
        {
            PropertyInvocationHandlerEventArgs e = new PropertyInvocationHandlerEventArgs(property, null, false);

            return this.invocationHandler.HandleOnPropertyInvoked(invocationHandler, e);
        }

        public object ProcessPropertySetInvocation(PropertyInfo property, object value)
        {
            PropertyInvocationHandlerEventArgs e = new PropertyInvocationHandlerEventArgs(property, value, true);

            return this.invocationHandler.HandleOnPropertyInvoked(invocationHandler, e);
        }
    }
}
