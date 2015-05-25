using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{

    void ProcessAssembly()
    {

        if (ModuleDefinition.Assembly.ContainsTimeAttribute() || ModuleDefinition.ContainsTimeAttribute())
        {
            foreach (var type in types)
            {
                if (type.IsInterceptor())
                {
                    continue;
                }
                if (type.IsCompilerGenerated())
                {
                    continue;
                }
                foreach (var method in type.ConcreteMethods())
                {
                    ProcessMethod(method);
                }
            }
            return;
        }

        foreach (var type in types)
        {
            if (type.IsInterceptor())
            {
                continue;
            }
            if (type.IsCompilerGenerated())
            {
                continue;
            }
            if (type.ContainsTimeAttribute())
            {
                foreach (var method in type.ConcreteMethods())
                {
                    ProcessMethod(method);
                }
                continue;
            }
            foreach (var method in type.ConcreteMethods()
                                       .Where(x => x.ContainsTimeAttribute()))
            {
                ProcessMethod(method);
            }
        }
    }


    void ProcessMethod(MethodDefinition method)
    {
        if (method.IsYield())
        {
            if (method.ContainsTimeAttribute())
            {
                LogError("Could not process '" + method.FullName + "' since methods that yield are currently not supported. Please remove the [Time] attribute from that method.");
                return;
            }
            LogInfo("Skipping '" + method.FullName + "' since methods that yield are not supported.");
            return;
        }


        if (method.IsAsync())
        {
            var asyncProcessor = new AsyncMethodProcessor
            {
                ModuleWeaver = this,
                Method = method,
            };

            asyncProcessor.Process();
            return;
        }
        var methodProcessor = new MethodProcessor
        {
            ModuleWeaver = this,
            Method = method,
        };

        methodProcessor.Process();
    }
}