public partial class ModuleWeaver
{
    void CheckForBadAttributes()
    {
        foreach (var typeDefinition in types)
        {
            foreach (var method in typeDefinition.AbstractMethods())
            {
                if (method.ContainsTimeAttribute())
                {
                    WriteError($"Method '{method.FullName}' is abstract but has a [TimeAttribute]. Remove this attribute.");
                }
            }
        }
    }
}