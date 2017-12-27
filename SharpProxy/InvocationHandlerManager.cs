using System;
namespace SharpProxy
{
    public class InvocationHandlerManager
    {
        public InvocationHandlerManager()
        {
        }

        public event MethodInvocationHandler OnMethodInvoked;

        public event PropertyInvocationHandler OnPropertyInvoked;

        internal object HandleOnMethodInvoked(object target, MethodInvocationHandlerEventArgs e)
        {
            return this.OnMethodInvoked(target, e);
        }

        internal object HandleOnPropertyInvoked(object target, PropertyInvocationHandlerEventArgs e)
        {
            return this.OnPropertyInvoked(target, e);
        }
    }
}
