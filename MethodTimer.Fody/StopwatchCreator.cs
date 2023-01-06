using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    public void InjectStopwatchType()
    {
        var type = new TypeDefinition(
            "MethodTimer",
            "Stopwatch",
            TypeAttributes.BeforeFieldInit | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
            TypeSystem.ObjectReference);
        ModuleDefinition.Types.Add(type);

        var startTicks = new FieldDefinition("startTicks", FieldAttributes.Private, TypeSystem.Int64Reference);
        type.Fields.Add(startTicks);

        var stopped = new FieldDefinition("stopped", FieldAttributes.Private, TypeSystem.BooleanReference);
        type.Fields.Add(stopped);

        var elapsedTicks = new FieldDefinition("elapsedTicks", FieldAttributes.Private, TypeSystem.Int64Reference);
        type.Fields.Add(elapsedTicks);

        var currentTicks = new MethodDefinition(
            "CurrentTicks",
            MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.Static,
            TypeSystem.Int64Reference)
        {
            Body =
            {
                InitLocals = true
            }
        };
        type.Methods.Add(currentTicks);
        var timeVariable = new VariableDefinition(DateTimeType);
        currentTicks.Body.Variables.Add(timeVariable);
        currentTicks.Body.Add(
            Instruction.Create(OpCodes.Call, UtcNowMethod),
            Instruction.Create(OpCodes.Stloc, timeVariable),
            Instruction.Create(OpCodes.Ldloca_S, timeVariable),
            Instruction.Create(OpCodes.Call, GetTicksMethod),
            Instruction.Create(OpCodes.Ret));

        var constructor = new MethodDefinition(
            ".ctor",
            MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
            TypeSystem.VoidReference);
        type.Methods.Add(constructor);
        constructor.Body.Add(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, currentTicks),
            Instruction.Create(OpCodes.Stfld, startTicks),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, ObjectConstructorMethod),
            Instruction.Create(OpCodes.Ret));

        var startNew = new MethodDefinition(
            "StartNew",
            MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static,
            type);
        type.Methods.Add(startNew);
        startNew.Body.Add(
            Instruction.Create(OpCodes.Newobj, constructor),
            Instruction.Create(OpCodes.Ret));

        var stop = new MethodDefinition(
            "Stop",
            MethodAttributes.HideBySig | MethodAttributes.Public,
            TypeSystem.VoidReference);
        type.Methods.Add(stop);
        var stopReturn = Instruction.Create(OpCodes.Ret);
        stop.Body.Add(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, stopped),
            Instruction.Create(OpCodes.Brtrue, stopReturn),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldc_I4_1),
            Instruction.Create(OpCodes.Stfld, stopped),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldc_I4_0),
            Instruction.Create(OpCodes.Conv_I8),
            Instruction.Create(OpCodes.Call, currentTicks),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, startTicks),
            Instruction.Create(OpCodes.Sub),
            Instruction.Create(OpCodes.Call, MaxMethod),
            Instruction.Create(OpCodes.Stfld, elapsedTicks),
            stopReturn);

        var elapsedMilliseconds = new MethodDefinition(
            "GetElapsedMilliseconds",
            MethodAttributes.HideBySig | MethodAttributes.Public,
            TypeSystem.Int64Reference);
        type.Methods.Add(elapsedMilliseconds);
        elapsedMilliseconds.Body.Add(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, stop),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, elapsedTicks),
            Instruction.Create(OpCodes.Ldc_I4, 10000),
            Instruction.Create(OpCodes.Conv_I8),
            Instruction.Create(OpCodes.Div),
            Instruction.Create(OpCodes.Ret));
        ElapsedMilliseconds = elapsedMilliseconds;

        var elapsed = new MethodDefinition(
            "GetElapsed",
            MethodAttributes.HideBySig | MethodAttributes.Public,
            TimeSpanType);
        type.Methods.Add(elapsed);
        elapsed.Body.Add(
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Call, stop),
            Instruction.Create(OpCodes.Ldarg_0),
            Instruction.Create(OpCodes.Ldfld, elapsedTicks),
            Instruction.Create(OpCodes.Newobj, TimeSpanConstructorMethod),
            Instruction.Create(OpCodes.Ret));
        
        
        IsRunning = ModuleDefinition.ImportReference(stopwatchType.Method("get_IsRunning"));

        Elapsed = elapsed;
        StopMethod = stop;
        StopwatchType = type;
        StartNewMethod = startNew;
    }
}