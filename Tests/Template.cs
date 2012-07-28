using System;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class Template
{
    [Test]
    [Ignore]
    public void LookAtIL()
    {
        var moduleDefinition = ModuleDefinition.ReadModule(GetType().Assembly.Location);
        var methods = moduleDefinition.GetType("Tempalate").Methods;
        var noWeaving = methods.First(x=>x.Name=="NoWeaving").Body;
        var withWeaving = methods.First(x => x.Name == "WithWeaving").Body;
        Debug.WriteLine(noWeaving);
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