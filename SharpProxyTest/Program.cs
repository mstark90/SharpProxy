using System;
using System.Collections.Generic;
using SharpProxy;

namespace SharpProxyTest
{
    class Program
    {
        private static Dictionary<string, string> data;
        static void Main(string[] args)
        {
            data = new Dictionary<string, string>();

            InvocationHandlerManager invocationManager = new InvocationHandlerManager();

            invocationManager.OnMethodInvoked += new MethodInvocationHandler(MethodCalled);
            invocationManager.OnPropertyInvoked += InvocationManager_OnPropertyInvoked;

            IConsoleWriter writer = Proxy.Get<IConsoleWriter>(invocationManager, typeof(IConsoleWriter));

            Proxy.SaveGeneratedProxies("SharpProxyContainer.dll");

            writer.Value = "Michael";

            writer.Write("Hello, {0}!");

            writer.Write("Hello, {0}!");
        }

        private static object InvocationManager_OnPropertyInvoked(object target, PropertyInvocationHandlerEventArgs e)
        {
            if (e.IsUpdating)
            {
                if (data.ContainsKey(e.InvokedProperty.Name))
                {
                    data[e.InvokedProperty.Name] = (string)e.Value;
                }
                else
                {
                    data.Add(e.InvokedProperty.Name, (string)e.Value);
                }

                return null;
            }
            else
            {
                return data[e.InvokedProperty.Name];
            }
        }

        static object MethodCalled(object target, MethodInvocationHandlerEventArgs e)
        {
            Console.Write(String.Format((string)e.Parameters["value"], data["Value"]));

            return null;
        }
    }
}
