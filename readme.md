# <img src="/package_icon.png" height="30px"> MethodTimer.Fody

[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg)](https://gitter.im/Fody/Fody)
[![NuGet Status](https://img.shields.io/nuget/v/MethodTimer.Fody.svg)](https://www.nuget.org/packages/MethodTimer.Fody/)

Injects some very basic method timing code.


### This is an add-in for [Fody](https://github.com/Fody/Home/)

**It is expected that all developers using Fody either [become a Patron on OpenCollective](https://opencollective.com/fody/), or have a [Tidelift Subscription](https://tidelift.com/subscription/pkg/nuget-fody?utm_source=nuget-fody&utm_medium=referral&utm_campaign=enterprise).**


## Usage

See also [Fody usage](https://github.com/Fody/Home/blob/master/pages/usage.md).


### NuGet installation

Install the [MethodTimer.Fody NuGet package](https://nuget.org/packages/MethodTimer.Fody/) and update the [Fody NuGet package](https://nuget.org/packages/Fody/):

```powershell
PM> Install-Package Fody
PM> Install-Package MethodTimer.Fody
```

The `Install-Package Fody` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.


### Add to FodyWeavers.xml

Add `<MethodTimer/>` to [FodyWeavers.xml](https://github.com/Fody/Home/blob/master/pages/usage.md#add-fodyweaversxml)

```xml
<Weavers>
  <MethodTimer/>
</Weavers>
```


### Your Code

```csharp
public class MyClass
{
    [Time]
    public void MyMethod()
    {
        //Some code u are curious how long it takes
        Console.WriteLine("Hello");
    }
}
```


### What gets compiled without an Interceptor

```csharp
public class MyClass
{
    public void MyMethod()
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
            Trace.WriteLine("MyClass.MyMethod " + stopwatch.ElapsedMilliseconds + "ms");
        }
    }
}
```


### What gets compiled with an Interceptor

If you want to handle the logging you can define a static class to intercept the logging. 

The interceptor takes one of the two following forms.

**Note:** when both methods are available, the intercepter will prefer the `TimeSpan` overload.


#### Interceptor with elapsed duration as long (milliseconds) 

```csharp
public static class MethodTimeLogger
{
    public static void Log(MethodBase methodBase, long milliseconds, string message)
    {
        //Do some logging here
    }
}
```

Then this will be compiled

```csharp
public class MyClass
{
    public void MyMethod()
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
            MethodTimeLogger.Log(methodof(MyClass.MyMethod), stopwatch.ElapsedMilliseconds);
        }
    }
}
```


#### Interceptor with elapsed duration as TimeSpan

```csharp
public static class MethodTimeLogger
{
    public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
    {
        //Do some logging here
    }
}
```

Then this will be compiled

```csharp
public class MyClass
{
    public void MyMethod()
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
            MethodTimeLogger.Log(methodof(MyClass.MyMethod), stopwatch.Elapsed);
        }
    }
}
```


### Using parameters inside the logging

If you want to get the parameter values inside the logging, you can use a string format in the attribute definition.

```csharp
public class MyClass
{
    [Time("File name: '{fileName}'")]
    public void MyMethod(string fileName)
    {
        //Some code u are curious how long it takes
        Console.WriteLine("Hello");
    }
}
```

Then this will be compiled

```csharp
public class MyClass
{
    public void MyMethod(string fileName)
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
            var message = string.Format("File name: '{0}'", fileName);
            MethodTimeLogger.Log(methodof(MyClass.MyMethod), stopwatch.ElapsedMilliseconds, message);
        }
    }
}
```

The following values are allowed:

* Any parameter name (e.g. `{fileName}`)
* `{this}` (calls `ToString()` on the instance itself) - Note that this is not available on static methods, the weaver will throw an error if being used in a static method

**Note 1:** sub-properties are not (yet?) supported. [Support Fody on OpenCollective](https://opencollective.com/fody) and this might be implemented!

**Note 2:** this feature requires an updated Log method call with the definition below. If this method (with the *message* parameter) is not found, the weaver will raise an error.

```
public static void Log(MethodBase methodBase, long milliseconds, string message)
```


## Whats in the NuGet

In addition to the actual weaving assembly the NuGet package will also add a file `TimeAttribute.cs` to the target project.

```csharp
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor,AllowMultiple = false)]
class TimeAttribute : Attribute
{
}
```

At compile time this attribute and all usages to it will be removed from the target assembly. If you want to re-use the class in a common assembly change the class from `internal` to `public`. This will result in the class not being removed at compile time.


## Icon

Icon courtesy of [The Noun Project](https://thenounproject.com)