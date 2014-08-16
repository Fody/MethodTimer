using System;
using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodReference LogMethod;

    public void FindInterceptor(bool overwrite = true)
    {
        //If the interceptor is already defined, only overwrite if the flag is set.
        if(LogMethod != null && !overwrite)
            return;
        
        var interceptor = types.FirstOrDefault(x => x.Name == "MethodTimeLogger");
        if (interceptor == null)
        {
            foreach (var referencePath in ReferenceCopyLocalPaths)
            {
                if (!referencePath.EndsWith(".dll") && !referencePath.EndsWith(".exe"))
                {
                    continue;
                }
                var moduleDefinition = ReadModule(referencePath);

                interceptor = moduleDefinition
                    .GetTypes()
                    .FirstOrDefault(x => x.Name == "MethodTimeLogger");
                if (interceptor != null)
                {
                    interceptor = ModuleDefinition.Import(interceptor).Resolve();
                    break;
                }
            }
        }
        if (interceptor == null)
        {
            return;
        }
        var logMethod = interceptor.Methods.FirstOrDefault(x => x.Name == "Log");
        if (logMethod == null)
        {
            throw new WeavingException(string.Format("Could not find 'Log' method on '{0}'.", interceptor.FullName));
        }
        if (!logMethod.IsPublic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' is not public.");
        }
        if (!logMethod.IsStatic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' is not static.");
        }
        var parameters = logMethod.Parameters;
        if (parameters.Count != 2)
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.");
        }
        if (parameters[0].ParameterType.FullName != "System.Reflection.MethodBase")
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.");
        }
        if (parameters[1].ParameterType.FullName != "System.Int64")
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.");
        }

        LogMethod = ModuleDefinition.Import(logMethod);
    }

    ModuleDefinition ReadModule(string referencePath)
    {
        var readerParameters = new ReaderParameters
            {
                AssemblyResolver = AssemblyResolver
            };
        try
        {
            return ModuleDefinition.ReadModule(referencePath, readerParameters);
        }
        catch (Exception exception)
        {
            var message = string.Format("Failed to read {0}. {1}", referencePath, exception.Message);
            throw new Exception(message, exception);
        }
    }
}