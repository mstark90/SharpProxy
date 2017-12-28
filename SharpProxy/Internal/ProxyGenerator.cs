using System;
using System.Reflection;
using System.Reflection.Emit;

namespace SharpProxy.Internal
{
    internal class ProxyGenerator
    {
        private AssemblyBuilder proxyContainer;
        private ModuleBuilder proxyModule;
        private Type proxyBaseType;
        private MethodImplementationGenerator methodImplGen;

        public ProxyGenerator()
        {
            AssemblyName assemblyName = new AssemblyName("SharpProxyContainer");

            this.proxyContainer = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);

            this.proxyModule = this.proxyContainer.DefineDynamicModule("SharpProxyContainer.dll");

            this.proxyBaseType = typeof(SharpProxyBase);

            this.methodImplGen = new MethodImplementationGenerator();
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

        public AssemblyBuilder GeneratedAssembly
        {
            get
            {
                return proxyContainer;
            }
        }

        public Type CreateProxy(Type[] interfaces)
        {
            if(interfaces.Length == 0) {
                return null;
            }

            Type _type = this.proxyModule.GetType(interfaces[0].Name + "_Proxy");

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

            TypeBuilder proxyType = this.proxyModule.DefineType(interfaces[0].Name + "_Proxy",
                                                                TypeAttributes.Public | TypeAttributes.Class,
                                                                proxyBaseType, interfaces);

            ImplementConstructor(proxyType);

            foreach(Type interfaceType in interfaces)
            {

                foreach(MethodInfo method in interfaceType.GetMethods())
                {
                    if (!method.IsSpecialName)
                    {
                        ImplementMethodCall(proxyType, method);
                    }
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
            ConstructorBuilder ctorBuilder = proxyType.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig,
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

            MethodBuilder methodBuilder = proxyType.DefineMethod(parentMethod.Name,
                                                                 MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot,
                                                                 parentMethod.ReturnType, paramTypes);

            for (int i = 0; i < methodParams.Length; i++)
            {
                ParameterBuilder paramBuilder = methodBuilder.DefineParameter(i, methodParams[i].Attributes, methodParams[i].Name);
            }

            ILGenerator ilGen = methodBuilder.GetILGenerator();

            LocalBuilder methodArgs = methodImplGen.EmitParameterProcessCode(ilGen, parentMethod);

            this.methodImplGen.EmitMethodCallHandlerInvocation(ilGen, parentMethod, methodArgs, proxyBaseType.GetMethod("ProcessMethodInvocation"));

            return methodBuilder;
        }

        private PropertyBuilder ImplementPropertyCall(TypeBuilder proxyType, PropertyInfo parentProperty)
        {
            Type[] indexParms = new Type[parentProperty.GetIndexParameters().Length];

            for (int i = 0; i < indexParms.Length; i++)
            {
                indexParms[i] = parentProperty.GetIndexParameters()[i].ParameterType;
            }

            PropertyBuilder propBuilder = proxyType.DefineProperty(parentProperty.Name,
                                                                   parentProperty.Attributes,
                                                                   parentProperty.PropertyType,
                                                                   indexParms);

            if (parentProperty.SetMethod != null)
            {
                ParameterInfo[] propertyParams = parentProperty.SetMethod.GetParameters();
                Type[] paramTypes = new Type[propertyParams.Length];

                for (int i = 0; i < propertyParams.Length; i++)
                {
                    paramTypes[i] = propertyParams[i].ParameterType;
                }

                MethodBuilder setMethod = proxyType.DefineMethod(parentProperty.SetMethod.Name,
                                                                 MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot,
                                                                 parentProperty.SetMethod.ReturnType, paramTypes);

                ILGenerator ilGen = setMethod.GetILGenerator();

                methodImplGen.EmitPropertyCallInvocation(ilGen, parentProperty, proxyBaseType.GetMethod("ProcessPropertySetInvocation"), true);

                propBuilder.SetSetMethod(setMethod);
            }

            if (parentProperty.GetMethod != null)
            {
                ParameterInfo[] propertyParams = parentProperty.GetMethod.GetParameters();
                Type[] paramTypes = new Type[propertyParams.Length];

                for (int i = 0; i < propertyParams.Length; i++)
                {
                    paramTypes[i] = propertyParams[i].ParameterType;
                }

                MethodBuilder getMethod = proxyType.DefineMethod(parentProperty.GetMethod.Name,
                                                                 MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot,
                                                                 parentProperty.GetMethod.ReturnType, paramTypes);

                ILGenerator ilGen = getMethod.GetILGenerator();

                methodImplGen.EmitPropertyCallInvocation(ilGen, parentProperty, proxyBaseType.GetMethod("ProcessPropertyGetInvocation"), false);

                propBuilder.SetGetMethod(getMethod);
            }

            return propBuilder;
        }
    }
}
