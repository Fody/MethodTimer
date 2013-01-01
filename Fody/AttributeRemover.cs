public partial class ModuleWeaver
{
    void RemoveAttributes()
    {
        foreach (var typeDefinition in types)
        {
            typeDefinition.RemoveTimeAttribute();
            foreach (var method in typeDefinition.Methods)
            {
                method.RemoveTimeAttribute();
            }
        }
    }
}