public partial class ModuleWeaver
{

    void CheckForBadAttributes()
    {
        foreach (var typeDefinition in types)
        {
            foreach (var method in typeDefinition.AbstractMethods())
            {
                if (method.ContainsAttribute(AttributeName))
                {
                    LogError(string.Format("Method '{0}' is abstract but has a [TimeAttribute]. Remove this attribute.", method.FullName));
                }
            }
        }
    }
}