using Mono.Cecil;

public partial class AsyncMethodProcessor
{
    public FieldDefinition InjectStartTimestamp(TypeDefinition typeDefinition)
    {
        var startTimestampField = new FieldDefinition(
            "_MethodTimer_StartTimestamp",
            FieldAttributes.Private,
            ModuleWeaver.TypeSystem.Int64Reference);

        typeDefinition.Fields.Add(startTimestampField);

        return startTimestampField;
    }

    public FieldDefinition InjectEndTimestamp(TypeDefinition typeDefinition)
    {
        // inject as variable
        var endTimestampField = new FieldDefinition(
            "_MethodTimer_EndTimestamp",
            FieldAttributes.Private,
            ModuleWeaver.TypeSystem.Int64Reference);

        typeDefinition.Fields.Add(endTimestampField);

        return endTimestampField;
    }

    public FieldDefinition InjectDurationTimestamp(TypeDefinition typeDefinition)
    {
        // inject as variable
        var durationTimestampField = new FieldDefinition(
            "_MethodTimer_DurationTimestamp",
            FieldAttributes.Private,
            ModuleWeaver.TypeSystem.Int64Reference);

        typeDefinition.Fields.Add(durationTimestampField);

        return durationTimestampField;
    }

    public FieldDefinition InjectDurationTimespan(TypeDefinition typeDefinition)
    {
        // inject as variable
        var durationTimespanField = new FieldDefinition("_MethodTimer_DurationTimespan",
            FieldAttributes.Private,
            ModuleWeaver.TimeSpanType);

        typeDefinition.Fields.Add(durationTimespanField);

        return durationTimespanField;
    }
}