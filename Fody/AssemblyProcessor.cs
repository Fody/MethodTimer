using System.Linq;

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
            if (typeDefinition.ContainsTimeAttribute())
            {
                methodProcessor.Process(typeDefinition.Methods.Where(x => x.IsMethodWithBody()));
                continue;
            }
            foreach (var method in typeDefinition.Methods)
            {
                if (!method.IsMethodWithBody())
                {
                    continue;
                }
                if (!method.ContainsTimeAttribute())
                {
                    continue;
                }
                methodProcessor.Process(method);
            }
        }
    }

}