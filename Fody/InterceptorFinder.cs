using System.Linq;
using Mono.Cecil;

public class InterceptorFinder
{
    public ModuleDefinition ModuleDefinition;
    public MethodDefinition LogMethod;


    public void Execute()
    {
        var errorHandler = ModuleDefinition.Types.FirstOrDefault(x => x.Name == "MethodTimeLogger");
        if (errorHandler == null)
        {
            return;
        }
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
            throw new WeavingException("Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodInfo' and 'System.Int64'.");
        }
        if (parameters[0].ParameterType.FullName != "System.Reflection.MethodInfo")
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodInfo' and 'System.Int64'.");
        }
        if (parameters[1].ParameterType.FullName != "System.Int64")
        {
            throw new WeavingException("Method 'MethodTimeLogger.Log' must have 2 parameters of type 'System.Reflection.MethodInfo' and 'System.Int64'.");
        }

    }

}