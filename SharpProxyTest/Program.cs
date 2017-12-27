using System;

using SharpProxy;

namespace SharpProxyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            InvocationHandlerManager invocationManager = new InvocationHandlerManager();

            invocationManager.OnMethodInvoked += new MethodInvocationHandler(MethodCalled);

            IConsoleWriter writer = new IConsoleWriterProxy(invocationManager);

            writer.Write("Hello World!");
        }

        static object MethodCalled(object target, MethodInvocationHandlerEventArgs e)
        {
            Console.WriteLine(e.Parameters["value"]);

            return null;
        }
    }
}
