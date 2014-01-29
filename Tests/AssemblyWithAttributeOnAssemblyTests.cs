﻿using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class AssemblyWithAttributeOnAssemblyTests
{
	AssemblyWeaver assemblyWeaver;

	public AssemblyWithAttributeOnAssemblyTests()
	{
		var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\DebugWithAttributeOnAssembly\AssemblyToProcess.dll");
		assemblyWeaver = new AssemblyWeaver(assemblyPath);
	}

	[Test]
	public void ClassWithNoAttribute()
	{
		var message = DebugRunner.CaptureDebug(() =>
			{
				var type = assemblyWeaver.Assembly.GetType("ClassWithNoAttribute");
				var instance = (dynamic) Activator.CreateInstance(type);
				instance.Method();
            });
        Assert.AreEqual(2, message.Count);
        Assert.IsTrue(message.First().StartsWith("ClassWithNoAttribute.Method "));
        Assert.IsTrue(message.Last().StartsWith("ClassWithNoAttribute.Method "));
	}

	[Test]
	public void PeVerify()
	{
		Verifier.Verify(assemblyWeaver.Assembly.CodeBase.Remove(0, 8));
	}

}