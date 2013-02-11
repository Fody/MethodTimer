//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Mono.Cecil;
//using Mono.Cecil.Cil;
//using Mono.Cecil.Rocks;

//public class MethodProcessorAsync
//{
//    public ModuleWeaver ModuleWeaver;
//    public TypeSystem TypeSystem;
//    public TypeDefinition AsyncTypeReference;
//    MethodBody body;
//    FieldDefinition stopwatchField;
//    public MethodDefinition OriginalMethod;
//    MethodDefinition moveNextMethod;

//    public void Process()
//    {
//        try
//        {
//            InnerProcess();
//        }
//        catch (Exception exception)
//        {
//            throw new WeavingException(string.Format("An error occurred processing '{0}'. Error: {1}", OriginalMethod.FullName, exception.Message));
//        }
//    }

//    void InnerProcess()
//    {
//        moveNextMethod = AsyncTypeReference.Methods.First(x => x.Name == "MoveNext");
//        body = moveNextMethod.Body;
//        body.SimplifyMacros();
//        InjectStopwatch();
//        var exceptionHandler = body.ExceptionHandlers.First();
//        var indexOf = body.Instructions.IndexOf(exceptionHandler.TryEnd) - 2;
//        foreach (var instruction in GetWriteTimeIL())
//        {
//            indexOf++;
//            body.Instructions.Insert(indexOf, instruction);
//        }
//        body.InitLocals = true;
//        body.OptimizeMacros();
//    }

//    IEnumerable<Instruction> GetWriteTimeIL()
//    {
//        yield return Instruction.Create(OpCodes.Ldloc, stopwatchField);
//        yield return Instruction.Create(OpCodes.Call, ModuleWeaver.StopMethod);
//        if (ModuleWeaver.LogMethod == null)
//        {
//            yield return Instruction.Create(OpCodes.Ldstr, OriginalMethod.MethodName());
//            yield return Instruction.Create(OpCodes.Ldloc, stopwatchField);
//            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
//            yield return Instruction.Create(OpCodes.Box, TypeSystem.Int64);
//            yield return Instruction.Create(OpCodes.Ldstr, "ms");
//            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ConcatMethod);
//            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.DebugWriteLineMethod);
//        }
//        else
//        {
//            yield return Instruction.Create(OpCodes.Ldtoken, OriginalMethod.MethodName());
//            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.GetMethodFromHandle);
//            yield return Instruction.Create(OpCodes.Ldloc, stopwatchField);
//            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.ElapsedMilliseconds);
//            yield return Instruction.Create(OpCodes.Call, ModuleWeaver.LogMethod);
//        }
//    }


//    void InjectStopwatch()
//    {
//        stopwatchField = new FieldDefinition("methodTimerStopwatch", FieldAttributes.Private, ModuleWeaver.StopwatchType);
//        AsyncMethod.DeclaringType.Fields.Add(stopwatchField);
//        var firstOrDefault = AsyncTypeReference.GetConstructors().FirstOrDefault(x=>x.);
//        body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, ModuleWeaver.StartNewMethod));
//        body.Instructions.Insert(1, Instruction.Create(OpCodes.Stloc, stopwatchField));
//    }
//}