public class AssemblyProcessor
{
    AllTypesFinder typesFinder;
    MethodProcessor methodProcessor;

    public AssemblyProcessor(AllTypesFinder typesFinder, MethodProcessor methodProcessor)
    {
        this.typesFinder = typesFinder;
        this.methodProcessor = methodProcessor;
    }

    public void Execute()
    {
        foreach (var typeDefinition in typesFinder.AllTypes)
        {
            methodProcessor.ProcessAttributes(typeDefinition);
            foreach (var method in typeDefinition.Methods)
            {
                methodProcessor.ProcessAttributes(method);
            }
        }
    }

}