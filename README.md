## This is an add-in for [Fody](https://github.com/SimonCropp/Fody/) 

Injects some very basic method timing code.

[Introduction to Fody](http://github.com/SimonCropp/Fody/wiki/SampleUsage)

## Nuget package http://nuget.org/packages/MethodTimer.Fody 

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

