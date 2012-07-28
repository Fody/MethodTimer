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


### What gets compiled

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
				Debug.WriteLine(string.Concat("MyClass.MyMethod", stopwatch.ElapsedMilliseconds));
			}
		}
	}