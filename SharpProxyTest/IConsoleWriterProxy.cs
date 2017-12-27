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

        public string Value
        {
            get
            {
                return (string)this.ProcessPropertyGetInvocation(typeof(IConsoleWriter).GetProperty("Value"));
            }
            set
            {
                this.ProcessPropertySetInvocation(typeof(IConsoleWriter).GetProperty("Value"), value);
            }
        }

        public void Write(string value)
        {
            object[] args = new object[2]
            {
                value,
                "test"
            };

            this.ProcessMethodInvocation(((MethodInfo)MethodBase.GetCurrentMethod()), args);
        }
    }
}
