using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpProxy.Internal
{
    internal sealed class MethodImplementationGenerator
    {
        private MethodInfo getPropertyHandle;
        public MethodImplementationGenerator()
        {
            this.getPropertyHandle = typeof(Type).GetMethod("GetProperty", new Type[] { typeof(string), typeof(Type) });
        }

        public LocalBuilder EmitParameterProcessCode(ILGenerator ilGen, MethodInfo method)
        {
            ParameterInfo[] methodParams = method.GetParameters();
            LocalBuilder methodArgs = ilGen.DeclareLocal(typeof(object[]));

            ilGen.Emit(OpCodes.Ldc_I4, methodParams.Length);
            ilGen.Emit(OpCodes.Newarr, typeof(object));

            for (int i = 0; i < methodParams.Length; i++)
            {
                ilGen.Emit(OpCodes.Dup);
                ilGen.Emit(OpCodes.Ldc_I4, i);
                ilGen.Emit(OpCodes.Ldarg, (short)(i + 1));
                ilGen.Emit(OpCodes.Stelem_Ref);
            }

            ilGen.Emit(OpCodes.Stloc, methodArgs);

            return methodArgs;
        }

        public void EmitMethodCallHandlerInvocation(ILGenerator ilGen,
                                                    MethodInfo parentMethod, LocalBuilder args, MethodInfo handler)
        {
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldtoken, parentMethod);
            ilGen.Emit(OpCodes.Ldtoken, parentMethod.DeclaringType);
            ilGen.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }));

            ilGen.Emit(OpCodes.Ldloc, args);

            ilGen.Emit(OpCodes.Call, handler);

            if (parentMethod.ReturnType == typeof(void))
            {
                ilGen.Emit(OpCodes.Pop);
            }

            ilGen.Emit(OpCodes.Ret);
        }

        private void EmitLoadPropertyInfo(ILGenerator ilGen, PropertyInfo property)
        {
            ilGen.Emit(OpCodes.Ldtoken, property.DeclaringType);
            ilGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
            ilGen.Emit(OpCodes.Ldstr, property.Name);
            ilGen.Emit(OpCodes.Ldtoken, property.PropertyType);
            ilGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
            ilGen.Emit(OpCodes.Call, this.getPropertyHandle);
        }

        public void EmitPropertyCallInvocation(ILGenerator ilGen,
                                               PropertyInfo property, MethodInfo handler, bool isSetMethod)
        {
            ilGen.Emit(OpCodes.Ldarg_0);

            EmitLoadPropertyInfo(ilGen, property);

            if (isSetMethod)
            {
                ilGen.Emit(OpCodes.Ldarg_1);
            }

            ilGen.Emit(OpCodes.Call, handler);

            if (isSetMethod)
            {
                ilGen.Emit(OpCodes.Pop);
            }

            ilGen.Emit(OpCodes.Ret);
        }
    }
}
