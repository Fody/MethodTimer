using System;
using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodReference LogMethod;

    public void FindInterceptor()
    {
        LogDebug(string.Format("Searching for an intercepter"));

        var interceptor = types.FirstOrDefault(x => x.IsInterceptor());
        if (interceptor != null)
        {
            var logMethod = interceptor.Methods.FirstOrDefault(x => x.Name == "Log");
            if (logMethod == null)
            {
                throw new WeavingException(string.Format("Could not find 'Log' method on '{0}'.", interceptor.FullName));
            }
            VerifyHasCorrectParameters(logMethod);
            VerifyMethodIsPublicStatic(logMethod);
            LogMethod = logMethod;
            return;
        }

        foreach (var referencePath in ReferenceCopyLocalPaths)
        {
            if (!referencePath.EndsWith(".dll") && !referencePath.EndsWith(".exe"))
            {
                continue;
            }

            ModuleDefinition moduleDefinition;

            try
            {
                moduleDefinition = ReadModule(referencePath);
            }
            catch (Exception)
            {
                LogDebug(string.Format("Failed to read module from '{0}', probably a .net native assembly", referencePath));
                continue;
            }

            interceptor = moduleDefinition
                .GetTypes()
                .FirstOrDefault(x => x.IsInterceptor());
            if (interceptor == null)
            {
                continue;
            }
            if (!interceptor.IsPublic)
            {
                LogInfo(string.Format("Did not use '{0}' since it is not public.", interceptor.FullName));
                continue;
            }
            var logMethod = interceptor.Methods.FirstOrDefault(x => x.Name == "Log");
            if (logMethod == null)
            {
                throw new WeavingException(string.Format("Could not find 'Log' method on '{0}'.", interceptor.FullName));
            }
            VerifyHasCorrectParameters(logMethod);
            VerifyMethodIsPublicStatic(logMethod);
            LogMethod = ModuleDefinition.ImportReference(logMethod);
            return;
        }
    }

// ReSharper disable once UnusedParameter.Local
    static void VerifyMethodIsPublicStatic(MethodDefinition logMethod)
    {
        if (!logMethod.IsPublic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' is not public.");
        }
        if (!logMethod.IsStatic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' is not static.");
        }
    }

    static void VerifyHasCorrectParameters(MethodDefinition logMethod)
    {
        var logMethodHasCorrectParameters = true;
        var parameters = logMethod.Parameters;
        if (parameters.Count != 2)
        {
            logMethodHasCorrectParameters = false;
        }
        if (parameters[0].ParameterType.FullName != "System.Reflection.MethodBase")
        {
            logMethodHasCorrectParameters = false;
        }
        if (parameters[1].ParameterType.FullName != "System.Int64")
        {
            logMethodHasCorrectParameters = false;
        }
        if (!logMethodHasCorrectParameters)
        {
            throw new WeavingException(string.Format("Method '{0}' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.", logMethod.FullName));
        }
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