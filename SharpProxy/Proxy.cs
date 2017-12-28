using System;

using SharpProxy.Internal;

namespace SharpProxy
{
    public class Proxy
    {
        public Proxy()
        {
        }

        public static void SaveGeneratedProxies(string fileName)
        {
            ProxyGenerator.DefaultGenerator.GeneratedAssembly.Save(fileName);
        }

        public static object Get(InvocationHandlerManager invocationManager, params Type[] interfaces)
        {
            Type proxyType = ProxyGenerator.DefaultGenerator.CreateProxy(interfaces);

            SharpProxyBase proxy = (SharpProxyBase)proxyType.GetConstructor(new Type[] { typeof(InvocationHandlerManager) }).Invoke(new object[] { invocationManager });

            return proxy;
        }

        public static T Get<T>(InvocationHandlerManager invocationManager, params Type[] interfaces) where T : class
        {
            return (T)Get(invocationManager, interfaces);
        }
    }
}
