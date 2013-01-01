using System;
using System.Diagnostics;
using System.Threading;
using MethodTimer;

[Time]
public class MiscMethods
{

    public void Method()
    {
        Thread.Sleep(10);
    }
    
    public int MethodWithReturn()
    {
        Thread.Sleep(10);
        return 0;
    }
    
    public int MethodWithReturnAndCatchReThrow()
    {
        try
        {
            Thread.Sleep(10);
            return 0;
        }
        catch (Exception)
        {
            Debug.WriteLine("sdf");
            throw;
        }
    }
    public void MethodAndCatchReThrow()
    {
        try
        {
            Thread.Sleep(10);
        }
        catch (Exception)
        {
            Debug.WriteLine("sdf");
            throw;
        }
    }
    public int MethodWithReturnAndCatchFinallyReThrow()
    {
        try
        {
            Thread.Sleep(10);
            return 0;
        }
        catch (Exception)
        {
            Debug.WriteLine("sdf");
            throw;
        }
        finally
        {
            Debug.WriteLine("sdf");
        }
    }
    public void MethodAndCatchFinallyReThrow()
    {
        try
        {
            Thread.Sleep(10);
        }
        catch (Exception)
        {
            Debug.WriteLine("sdf");
            throw;
        }
        finally
        {
            Debug.WriteLine("sdf");
        }
    }
    
    public void MethodAndCatchSwallow()
    {
        try
        {
            Thread.Sleep(10);
        }
        catch (Exception)
        {
        }
    }
    public int MethodWithReturnAndCatchSwallow()
    {
        try
        {
            Thread.Sleep(10);
            return 0;
        }
        catch (Exception)
        {
            return 1;
        }
    }

    public int MethodRecursive()
    {
        return MethodRecursive();
    }
}