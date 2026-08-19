using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Exp.Operations;

namespace Exp.Converting;

public static class Convert
{
    public static ExternFunc ToFunc(MethodInfo method, string? ns = null)
    {
        if (!method.CanBeConvertedToExpFunc(out var attr, out var error))
            throw new Exception(error);

        // create Func<...> from methodInfo
        var invoker = (Func<Instance?, IValue?[], IValue?>)Delegate.CreateDelegate(typeof(Func<Instance?, IValue?[], IValue?>), null, method);
        string invokerName = attr!.CustomName ?? method.Name.StartWithLowerCase();

        return new(invoker, attr.ParamsCounts, invokerName, ns);
    }

    public static ClassDefSpan ToClass<T>(string? ns, Interpreter interpreter) where T : Instance, IConvertable
    {
        Type type = typeof(T);

        List<Property> instanceProps = [];
        List<ClassStaticVar> staticProps = [];
        List<ConstructorDefSpan> ctors = [];
        const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;

        // get all properties of type Variable
        foreach (var property in type.GetProperties(bindingFlags))
        {
            // make sure its type is exp variable
            Type pType = property.PropertyType;
            if (pType != typeof(Variable) && !pType.IsSubclassOf(typeof(Variable)))
                continue;

            string name = property.Name.StartWithLowerCase();

            // create static var / instance property
            if (property.IsStatic)
                staticProps.Add(new(name, null, null, null));
            else
                instanceProps.Add(new(null, false, name, false, false));
        }

        // create the class
        ClassDefSpan cls = new(type.Name, [.. instanceProps], [.. ctors]) { Namespace = ns };
        cls.Vars.AddRange(staticProps);
        instanceProps.ForEach(p => p.Def = cls);
        staticProps.ForEach(p => p.Def = cls);

        // inject funcs with this signature: IValue(Instance?, IValue?[])
        foreach (var method in type.GetMethods(bindingFlags))
        {
            if (method.CanBeConvertedToExpFunc(out var attr, out var error))
                interpreter.AddExternFunc(ToFunc(method), cls, false);
            else if (attr != null) // if it's not marked [ExpFunc], just ignore this method
                throw new Exception(error);
        }


        // get methods with [ExpCtor] attr
        foreach (var ctorInfo in type.GetMethods(bindingFlags))
        {
            // make sure it's marked as [ExpCtor]
            if (ctorInfo.GetCustomAttribute<ExpCtorAttribute>() is not { } attr)
                continue;

            // make sure it's matching this signature: ctor(IValue?[])
            if (ctorInfo.GetParameters() is not { Length: 2 } param || param[0].ParameterType != typeof(Instance) || param[1].ParameterType != typeof(IValue?[]))
                continue;

            foreach (int paramOption in attr.ParamOptions)
            {
                // create the delegate
                var _delegate = (Func<Instance?, IValue?[], IValue?>)Delegate.CreateDelegate(typeof(Func<Instance?, IValue?[], IValue?>), null, ctorInfo);

                // create the ctor
                var prms = new ArgumentSpan[paramOption];
                for (uint i = 0; i < prms.Length; i++)
                    prms[i] = new("p" + (i + 1));
                ConstructorDefSpan ctor = new(prms, [], cls, interpreter) { OverridesInit = true };
                ctor.Operations = [new ReturnStatement(ctor, new ExternFuncInvocationOperation(ctor, new(_delegate, paramOption, ctor.Name)), null)];
                cls.Funcs = [.. cls.Funcs.Append(ctor)];
            }
        }

        //type.GetProperty(nameof(IConvertable.Class), BindingFlags.Public | BindingFlags.Static).SetValue(null, cls);
        T.Class = cls;
        return cls;
    }

    private static bool CanBeConvertedToExpFunc(this MethodInfo method, out ExpFuncAttribute? attr, out string? invalidReason)
    {
        invalidReason = null;
        attr = null;

        // make sure that the method is marked as [ExpFunc]
        if (method.GetCustomAttribute<ExpFuncAttribute>() is not { } _attr)
        {
            invalidReason = $"The method must be marked with the [{nameof(ExpFuncAttribute)}] attribute.";
            return false;
        }

        attr = _attr;

        // make sure that the method's signature matches this: IValue(Instance?, IValue?[])
        if (
            method.ReturnType != typeof(IValue) ||
            method.ReturnType.GetInterfaces().Contains(typeof(IValue)) ||
            method.ContainsGenericParameters ||
            method.GetParameters() is not { Length: 2 } mParams ||
            mParams[0].ParameterType != typeof(Instance) ||
            mParams[1].ParameterType != typeof(IValue[])
            )
        {
            invalidReason = $"Method signature must match this: {nameof(IValue)}({nameof(Instance)}?, {nameof(IValue)}?[]).";
            return false;
        }

        return true;
    }

    extension (PropertyInfo propertyInfo)
    {
        public bool IsStatic => (propertyInfo.GetMethod ?? propertyInfo.SetMethod)?.IsStatic ?? false;
    }
}