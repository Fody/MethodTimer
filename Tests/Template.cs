using System;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using NUnit.Framework;
// ReSharper disable UnusedMember.Global

[TestFixture]
public class Template
{
    [Test]
    [Ignore]
    public void LookAtIL()
    {
        var moduleDefinition = ModuleDefinition.ReadModule(GetType().Assembly.Location);
        var methods = moduleDefinition.GetType("Template").Methods;
        var noWeaving = methods.First(x=>x.Name=="NoWeaving").Body;
        var withWeaving = methods.First(x => x.Name == "WithWeaving").Body;
        Trace.WriteLine(noWeaving);
    }

    public void NoWeaving()
    {
        //Some code u are curious how long it takes
        Console.WriteLine("Hello");
    }

    public void WithWeaving()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            //Some code u are curious how long it takes
            Console.WriteLine("Hello");
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine(string.Concat("Template.WithWeaving", stopwatch.ElapsedMilliseconds));
        }
    }
#if (DEBUG)
    public async void AsyncWithWeaving()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            //Some code u are curious how long it takes
            Console.WriteLine("Hello");
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine(string.Concat("Template.WithWeaving", stopwatch.ElapsedMilliseconds));
        }
    }
#endif
    public void WithWeavingInterceptor()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            //Some code u are curious how long it takes
            Console.WriteLine("Hello");
        }
        finally
        {
            stopwatch.Stop();
            MethodTimeLogger.Log(typeof(Template),stopwatch.ElapsedMilliseconds);
        }
    }

    public string NoWeavingAndReturn()
    {
        Console.WriteLine("sdlkmflskdmf");
        return "sdf";
    }

    public object WithWeavingAndReturn()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Console.WriteLine("sdlkmflskdmf");
            return "sdf";
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine(string.Concat("Template.WithWeaving", stopwatch.ElapsedMilliseconds));
        }
    }
}
public static class MethodTimeLogger
{
    public static void Log(Type methodInfo, long milliseconds)
    {
    }
}