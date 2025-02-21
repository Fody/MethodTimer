using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    public void InjectMethodTimerHelper()
    {
        //.class private auto ansi abstract sealed beforefieldinit MethodTimerHelper

        //    extends[System.Runtime] System.Object
        //    {
        // // Fields
        // .field assembly static initonly float64 TimestampToTicks

        //    // Methods
        //    .method private hidebysig specialname rtspecialname static
        //        void .cctor() cil managed
        //    {
        //  // Method begins at RVA 0x20e9
        //  // Header size: 1
        //  // Code size: 22 (0x16)
        //  .maxstack 8

        //  // TimestampToTicks = (double)TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        //  IL_0000: ldc.r8 10000000
        //  IL_0009: ldsfld int64 [System.Runtime]
        //        System.Diagnostics.Stopwatch::Frequency
        //        IL_000e: conv.r8
        //        IL_000f: div
        //        IL_0010: stsfld float64 MethodTimerHelper::TimestampToTicks
        //        // }
        //        IL_0015: ret
        //    } // end of method MethodTimerHelper::.cctor

        //} // end of class MethodTimerHelper

        var type = new TypeDefinition(
            "MethodTimer",
            "MethodTimerHelper",
            TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.NotPublic,
            TypeSystem.ObjectReference);
        ModuleDefinition.Types.Add(type);

        var timestampToTicksField = new FieldDefinition("TimestampToTicks",
            FieldAttributes.Assembly | FieldAttributes.Static | FieldAttributes.InitOnly,
            Float64Type);
        type.Fields.Add(timestampToTicksField);

        var staticConstructor = new MethodDefinition(".cctor",
            MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static,
            VoidType);
        staticConstructor.Body.Add(
            // Note: it's a const (10000000)
            Instruction.Create(OpCodes.Ldc_R8, 10000000d),
            //Instruction.Create(OpCodes.Ldsfld, TimeSpan_TicksPerSecondField),
            //Instruction.Create(OpCodes.Conv_R8),
            Instruction.Create(OpCodes.Ldsfld, Stopwatch_GetFrequencyField),
            Instruction.Create(OpCodes.Conv_R8),
            Instruction.Create(OpCodes.Div),
            Instruction.Create(OpCodes.Stsfld, timestampToTicksField),
            Instruction.Create(OpCodes.Ret));

        type.Methods.Add(staticConstructor);

        MethodTimerHelperType = type;
        MethodTimerHelper_TimestampToTicks = timestampToTicksField;
    }
}