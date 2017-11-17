[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg?style=flat)](https://gitter.im/Fody/Fody)
[![NuGet Status](http://img.shields.io/nuget/v/MethodTimer.Fody.svg?style=flat)](https://www.nuget.org/packages/MethodTimer.Fody/)


## This is an add-in for [Fody](https://github.com/Fody/Fody/)

![Icon](https://raw.github.com/Fody/MethodTimer/master/package_icon.png)

Injects some very basic method timing code.

[Introduction to Fody](http://github.com/Fody/Fody/wiki/SampleUsage)


## The nuget package 

https://nuget.org/packages/MethodTimer.Fody/

    PM> Install-Package MethodTimer.Fody


### Your Code

	public class MyClass
	{
		[Time]
		public void MyMethod()
		{
			//Some code u are curious how long it takes
			Console.WriteLine("Hello");
		}
	}


### What gets compiled without an Interceptor

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
                Debug.WriteLine("MyClass.MyMethod " + stopwatch.ElapsedMilliseconds + "ms");
            }
        }
    }


### What gets compiled with an Interceptor

If you want to handle the logging you can define a static class to intercept the logging. 

The interceptor takes the following form.

    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, long milliseconds)
        {
            //Do some logging here
        }
    }
    
Then this will be compiled

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


### Using parameters inside the logging

If you want to get the parameter values inside the logging, you can use a string format in the attribute definition.

	public class MyClass
	{
		[Time("File name: '{fileName}'")]
		public void MyMethod(string fileName)
		{
			//Some code u are curious how long it takes
			Console.WriteLine("Hello");
		}
	}

Then this will be compiled

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

**Note that this feature requires an updated Log method call with the definition below. If this method (with the *message* parameter) is not found, the weaver will raise an error.**

	public static void Log(MethodBase methodBase, long milliseconds, string message)


## Whats in the NuGet

In addition to the actual weaving assembly the NuGet package will also add a file `TimeAttribute.cs` to the target project.

	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor,AllowMultiple = false)]
	internal class TimeAttribute : Attribute
	{
	}

At compile time this attribute and all usages to it will be removed from the target assembly. If you want to re-use the class in a common assembly change the class from `internal` to `public`. This will result in the class not being removed at compile time.


## Icon

Icon courtesy of [The Noun Project](http://thenounproject.com)
