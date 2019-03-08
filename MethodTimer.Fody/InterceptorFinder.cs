using System;
using System.Diagnostics;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    const string LongType = "System.Int64";
    const string TimeSpanType = "System.TimeSpan";

    public MethodReference LogMethodUsingLong;
    public MethodReference LogWithMessageMethodUsingLong;

    public MethodReference LogMethodUsingTimeSpan;
    public MethodReference LogWithMessageMethodUsingTimeSpan;

    public bool LogMethodIsNop;

    public void FindInterceptor()
    {
        LogDebug("Searching for an interceptor");

        var interceptor = types.FirstOrDefault(x => x.IsInterceptor());
        if (interceptor != null)
        {
            LogMethodUsingLong = FindLogMethod(interceptor, LongType);
            LogWithMessageMethodUsingLong = FindLogWithMessageMethod(interceptor, LongType);

            LogMethodUsingTimeSpan = FindLogMethod(interceptor, TimeSpanType);
            LogWithMessageMethodUsingTimeSpan = FindLogWithMessageMethod(interceptor, TimeSpanType);

            if (LogMethodUsingLong == null && LogWithMessageMethodUsingLong == null &&
                LogMethodUsingTimeSpan == null && LogWithMessageMethodUsingTimeSpan == null)
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

            var logMethodUsingLong = FindLogMethod(interceptor, LongType);
            if (logMethodUsingLong != null)
            {
                LogMethodUsingLong = ModuleDefinition.ImportReference(logMethodUsingLong);
            }

            var logWithMessageMethodUsingLong = FindLogWithMessageMethod(interceptor, LongType);
            if (logWithMessageMethodUsingLong != null)
            {
                LogWithMessageMethodUsingLong = ModuleDefinition.ImportReference(logWithMessageMethodUsingLong);
            }

            var logMethodUsingTimeSpan = FindLogMethod(interceptor, TimeSpanType);
            if (logMethodUsingTimeSpan != null)
            {
                LogMethodUsingTimeSpan = ModuleDefinition.ImportReference(logMethodUsingTimeSpan);
            }

            var logWithMessageMethodUsingTimeSpan = FindLogWithMessageMethod(interceptor, TimeSpanType);
            if (logWithMessageMethodUsingTimeSpan != null)
            {
                LogWithMessageMethodUsingTimeSpan = ModuleDefinition.ImportReference(logWithMessageMethodUsingTimeSpan);
            }

            if (LogMethodUsingLong == null && LogWithMessageMethodUsingLong == null &&
                LogMethodUsingTimeSpan == null && LogWithMessageMethodUsingTimeSpan == null)
            {
                throw new WeavingException($"Could not find 'Log' method on '{interceptor.FullName}'.");
            }
            return;
        }
    }

    MethodDefinition FindLogMethod(TypeDefinition interceptorType, string elapsedParameterTypeName)
    {
        var requiredParameterTypes = new[] { "System.Reflection.MethodBase", elapsedParameterTypeName };

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
        CheckNop(logMethod);

        return logMethod;
    }

    MethodDefinition FindLogWithMessageMethod(TypeDefinition interceptorType, string elapsedParameterTypeName)
    {
        var requiredParameterTypes = new[] { "System.Reflection.MethodBase", elapsedParameterTypeName, "System.String" };

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