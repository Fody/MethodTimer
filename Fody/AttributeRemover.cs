public partial class ModuleWeaver
{
    void RemoveAttributes()
    {
		ModuleDefinition.RemoveTimeAttribute();
		ModuleDefinition.Assembly.RemoveTimeAttribute();
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