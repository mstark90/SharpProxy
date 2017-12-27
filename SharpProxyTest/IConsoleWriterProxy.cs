using System;
using System.Reflection;

using SharpProxy;
using SharpProxy.Internal;

namespace SharpProxyTest
{
    public class IConsoleWriterProxy : SharpProxyBase, IConsoleWriter
    {
        public IConsoleWriterProxy(InvocationHandlerManager handlerManager) : base(handlerManager)
        {
        }

        public void Write(string value)
        {
            MethodInfo currentMethod = ((MethodInfo)MethodBase.GetCurrentMethod());

            this.ProcessMethodInvocation(currentMethod, value);
        }
    }
}
