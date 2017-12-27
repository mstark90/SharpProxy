﻿using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpProxy.Internal
{
    internal class ProxyGenerator
    {
        private AssemblyBuilder proxyContainer;
        private ModuleBuilder proxyModule;
        private Type proxyBaseType;

        public ProxyGenerator()
        {
            AssemblyName assemblyName = new AssemblyName("SharpProxyContainer");

            this.proxyContainer = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            this.proxyModule = this.proxyContainer.DefineDynamicModule("SharpProxyContainer.dll");

            this.proxyBaseType = typeof(SharpProxyBase);
        }

        private static ProxyGenerator m_DefaultGenerator;

        public static ProxyGenerator DefaultGenerator
        {
            get
            {
                if(m_DefaultGenerator == null)
                {
                    m_DefaultGenerator = new ProxyGenerator();
                }

                return m_DefaultGenerator;
            }
        }

        public Type CreateProxy(Type[] interfaces)
        {
            if(interfaces.Length == 0) {
                return null;
            }

            Type _type = this.proxyModule.GetType(interfaces[0].Name + "$Proxy");

            if(_type != null)
            {
                bool isFound = true;
                foreach(Type interfaceType in interfaces)
                {
                    if(!interfaceType.IsAssignableFrom(interfaceType))
                    {
                        isFound = false;
                        break;
                    }
                }

                if(isFound)
                {
                    return _type;
                }
            }

            TypeBuilder proxyType = this.proxyModule.DefineType(interfaces[0].Name + "$Proxy",
                                                                TypeAttributes.Public | TypeAttributes.Class,
                                                                proxyBaseType, interfaces);

            ImplementConstructor(proxyType);

            foreach(Type interfaceType in interfaces)
            {
                foreach(MethodInfo method in interfaceType.GetMethods())
                {
                    ImplementMethodCall(proxyType, method);
                }

                foreach(PropertyInfo property in interfaceType.GetProperties())
                {
                    ImplementPropertyCall(proxyType, property);
                }
            }

            return proxyType.CreateType();
        }

        private void ImplementConstructor(TypeBuilder proxyType)
        {
            ConstructorBuilder ctorBuilder = proxyType.DefineConstructor(MethodAttributes.Public,
                                                                         CallingConventions.Standard,
                                                                         new Type[] { typeof(InvocationHandlerManager) });

            ILGenerator ilGen = ctorBuilder.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Call, proxyBaseType.GetConstructor(new Type[] { typeof(InvocationHandlerManager) }));
            ilGen.Emit(OpCodes.Ret);
        }

        private MethodBuilder ImplementMethodCall(TypeBuilder proxyType, MethodInfo parentMethod)
        {
            ParameterInfo[] methodParams = parentMethod.GetParameters();
            Type[] paramTypes = new Type[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++)
            {
                paramTypes[i] = methodParams[i].ParameterType;
            }

            MethodBuilder methodBuilder = proxyType.DefineMethod(parentMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, parentMethod.ReturnType, paramTypes);

            ILGenerator ilGen = methodBuilder.GetILGenerator();

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.EmitCall(OpCodes.Call, typeof(MethodBase).GetMethod("GetCurrentMethod", BindingFlags.Public | BindingFlags.Static), null);

            for (int i = 0; i < methodParams.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, (short)(i + 1));
            }

            ilGen.EmitCall(OpCodes.Call, proxyBaseType.GetMethod("ProcessMethodInvocation"), null);
            ilGen.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        private PropertyBuilder ImplementPropertyCall(TypeBuilder proxyType, PropertyInfo parentProperty)
        {
            ParameterInfo[] propertyParams = parentProperty.SetMethod.GetParameters();
            Type[] paramTypes = new Type[propertyParams.Length];

            for (int i = 0; i < propertyParams.Length; i++)
            {
                paramTypes[i] = propertyParams[i].ParameterType;
            }

            PropertyBuilder propBuilder = proxyType.DefineProperty(parentProperty.Name,
                                                                   parentProperty.Attributes,
                                                                   parentProperty.GetMethod.ReturnType,
                                                                   paramTypes);

            if (parentProperty.SetMethod != null)
            {
                MethodBuilder setMethod = ImplementMethodCall(proxyType, parentProperty.SetMethod);
                propBuilder.SetSetMethod(setMethod);
            }

            if (parentProperty.GetMethod != null)
            {
                MethodBuilder getMethod = ImplementMethodCall(proxyType, parentProperty.GetMethod);
                propBuilder.SetGetMethod(getMethod);
            }

            return propBuilder;
        }
    }
}