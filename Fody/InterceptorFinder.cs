using System.Linq;
using Mono.Cecil;

public partial class ModuleWeaver
{
    public MethodDefinition LogMethod;
    public MethodDefinition LogOnMethodStartMethod;

    public void FindInterceptor()
    {
        var errorHandler = types.FirstOrDefault(x => x.Name == "MethodTimeLogger");
        if (errorHandler == null)
        {
            return;
        }
        ValidateLogMethod(errorHandler);
        ValidateLogOnMethodStart(errorHandler);
    }

    private void ValidateLogMethod(TypeDefinition errorHandler)
    {
        LogMethod = errorHandler.Methods.FirstOrDefault(x => x.Name == "Log");
        if (LogMethod == null)
        {
            throw new WeavingException(string.Format("Could not find 'Log' method on '{0}'.", errorHandler.FullName));
        }
        if (!LogMethod.IsPublic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' is not public.");
        }
        if (!LogMethod.IsStatic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' is not static.");
        }
        var parameters = LogMethod.Parameters;
        if (parameters.Count != 2)
        {
            throw new WeavingException(
                "Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.");
        }
        if (parameters[0].ParameterType.FullName != "System.Reflection.MethodBase")
        {
            throw new WeavingException(
                "Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.");
        }
        if (parameters[1].ParameterType.FullName != "System.Int64")
        {
            throw new WeavingException(
                "Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodBase' and 'System.Int64'.");
        }
    }

    private void ValidateLogOnMethodStart(TypeDefinition errorHandler)
    {
        LogOnMethodStartMethod = errorHandler.Methods.FirstOrDefault(x => x.Name == "LogOnMethodStart");
        if (LogOnMethodStartMethod == null)
        {
            throw new WeavingException(string.Format("Could not find 'LogOnMethodStart' method on '{0}'.", errorHandler.FullName));
        }
        if (!LogOnMethodStartMethod.IsPublic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.LogOnMethodStart' is not public.");
        }
        if (!LogOnMethodStartMethod.IsStatic)
        {
            throw new WeavingException("Method 'MethodTimeLogger.LogOnMethodStart' is not static.");
        }
        var parameters = LogOnMethodStartMethod.Parameters;
        if (parameters.Count != 1)
        {
            throw new WeavingException(
                "Method 'MethodTimeLogger.LogOnMethodStart' must have 1 parameter of type 'System.Reflection.MethodBase'.");
        }
        if (parameters[0].ParameterType.FullName != "System.Reflection.MethodBase")
        {
            throw new WeavingException(
                "Method 'MethodTimeLogger.LogOnMethodStart' must have 1 parameters of type 'System.Reflection.MethodBase'.");
        }
    }
}