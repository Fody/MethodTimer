using System;
using System.Diagnostics;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    public MethodReference LogMethod;
    public MethodReference LogWithMessageMethod;

    public bool LogMethodIsNop;

    public void FindInterceptor()
    {
        LogDebug("Searching for an interceptor");

        var interceptor = types.FirstOrDefault(x => x.IsInterceptor());
        if (interceptor != null)
        {
            LogMethod = FindLogMethod(interceptor);
            LogWithMessageMethod = FindLogWithMessageMethod(interceptor);

            if (LogMethod == null && LogWithMessageMethod == null)
            {
                throw new WeavingException($"Could not find 'Log' method on '{interceptor.FullName}'.");
            }
            return;
        }

        foreach (var referencePath in ReferenceCopyLocalPaths)
        {
            if (!referencePath.EndsWith(".dll") && !referencePath.EndsWith(".exe"))
            {
                continue;
            }

            var stopwatch = Stopwatch.StartNew();

            if (!Image.IsAssembly(referencePath))
            {
                LogDebug($"Skipped checking '{referencePath}' since it is not a .net assembly.");
                continue;
            }

            LogDebug($"Reading module from '{referencePath}'");
            var moduleDefinition = ReadModule(referencePath);

            stopwatch.Stop();

            interceptor = moduleDefinition
                .GetTypes()
                .FirstOrDefault(x => x.IsInterceptor());
            if (interceptor == null)
            {
                continue;
            }

            if (!interceptor.IsPublic)
            {
                LogInfo($"Did not use '{interceptor.FullName}' since it is not public.");
                continue;
            }

            var logMethod = FindLogMethod(interceptor);
            if (logMethod != null)
            {
                LogMethod = ModuleDefinition.ImportReference(logMethod);
            }

            var logWithMessageMethod = FindLogWithMessageMethod(interceptor);
            if (logWithMessageMethod != null)
            {
                LogWithMessageMethod = ModuleDefinition.ImportReference(logWithMessageMethod);
            }

            if (LogMethod == null && LogWithMessageMethod == null)
            {
                throw new WeavingException($"Could not find 'Log' method on '{interceptor.FullName}'.");
            }
            return;
        }
    }

    MethodDefinition FindLogMethod(TypeDefinition interceptorType)
    {
        var requiredParameterTypes = new[] { "System.Reflection.MethodBase", "System.Int64" };

        var logMethod = interceptorType.Methods.FirstOrDefault(x => x.Name == "Log" &&
                                                               x.Parameters.Count == 2 &&
                                                               x.Parameters[0].ParameterType.FullName == requiredParameterTypes[0] &&
                                                               x.Parameters[1].ParameterType.FullName == requiredParameterTypes[1]);
        if (logMethod == null)
        {
            return null;
        }

        VerifyHasCorrectParameters(logMethod, requiredParameterTypes);
        VerifyMethodIsPublicStatic(logMethod);
        LogMethod = ModuleDefinition.ImportReference(logMethod);
        CheckNop(logMethod);

        return logMethod;
    }

    MethodDefinition FindLogWithMessageMethod(TypeDefinition interceptorType)
    {
        var requiredParameterTypes = new[] { "System.Reflection.MethodBase", "System.Int64", "System.String" };

        var logMethod = interceptorType.Methods.FirstOrDefault(x => x.Name == "Log" &&
                                                                    x.Parameters.Count == 3 &&
                                                                    x.Parameters[0].ParameterType.FullName == requiredParameterTypes[0] &&
                                                                    x.Parameters[1].ParameterType.FullName == requiredParameterTypes[1] &&
                                                                    x.Parameters[2].ParameterType.FullName == requiredParameterTypes[2]);
        if (logMethod == null)
        {
            return null;
        }

        VerifyHasCorrectParameters(logMethod, requiredParameterTypes);
        VerifyMethodIsPublicStatic(logMethod);
        LogMethod = ModuleDefinition.ImportReference(logMethod);
        CheckNop(logMethod);

        return logMethod;
    }

    void CheckNop(MethodDefinition logMethod)
    {
        // Never reset
        if (LogMethodIsNop)
        {
            return;
        }

        LogMethodIsNop = logMethod.Body.Instructions.All(x =>
                x.OpCode == OpCodes.Nop ||
                x.OpCode == OpCodes.Ret
                );
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

    static void VerifyHasCorrectParameters(MethodDefinition logMethod, string[] expectedParameterTypes)
    {
        var logMethodHasCorrectParameters = true;
        var parameters = logMethod.Parameters;
        if (parameters.Count != expectedParameterTypes.Length)
        {
            logMethodHasCorrectParameters = false;
        }
        else
        {
            for (var i = 0; i < logMethod.Parameters.Count; i++)
            {
                if (parameters[i].ParameterType.FullName != expectedParameterTypes[i])
                {
                    logMethodHasCorrectParameters = false;
                }
            }
        }

        if (!logMethodHasCorrectParameters)
        {
            throw new WeavingException($"Method '{logMethod.FullName}' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.");
        }
    }

    ModuleDefinition ReadModule(string referencePath)
    {
        var readerParameters = new ReaderParameters
        {
            AssemblyResolver = ModuleDefinition.AssemblyResolver
        };

        try
        {
            return ModuleDefinition.ReadModule(referencePath, readerParameters);
        }
        catch (Exception exception)
        {
            var message = $"Failed to read {referencePath}. {exception.Message}";
            throw new Exception(message, exception);
        }
    }
}