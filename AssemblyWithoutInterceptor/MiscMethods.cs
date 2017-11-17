using System;
using System.Diagnostics;
using System.Threading;
using MethodTimer;

[Time]
public class MiscMethods
{


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
            Trace.WriteLine("sdf");
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
            Trace.WriteLine("sdf");
            throw;
        }

    }

    //public void Expected()
    //{
    //    var startNew = Stopwatch.StartNew();
    //    try
    //    {

    //        try
    //        {
    //            Thread.Sleep(10);
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }

    //    }
    //    finally
    //    {
    //        startNew.Stop();
    //    }
    //}

    public int MethodWithReturnAndCatchFinallyReThrow()
    {
        try
        {
            Thread.Sleep(10);
            return 0;
        }
        catch (Exception)
        {
            Trace.WriteLine("sdf");
            throw;
        }
        finally
        {
            Trace.WriteLine("sdf");
        }
    }
    //public void MethodAndCatchFinallyReThrow()
    //{
    //    try
    //    {
    //        Thread.Sleep(10);
    //    }
    //    catch (Exception)
    //    {
    //        Trace.WriteLine("sdf");
    //        throw;
    //    }
    //    finally
    //    {
    //        Trace.WriteLine("sdf");
    //    }
    //}

    //public void MethodAndCatchSwallow()
    //{
    //    try
    //    {
    //        Thread.Sleep(10);
    //    }
    //    catch (Exception)
    //    {
    //    }
    //}
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
    public int MethodWithThrowNewException()
    {
        try
        {
            Thread.Sleep(10);
            return 0;
        }
        catch (Exception e)
        {
            throw new Exception("dd", e);
        }
    }

    public int MultiCatch()
    {
        try
        {

            try
            {
                Thread.Sleep(10);
                return 0;
            }
            catch (Exception e)
            {
                throw new Exception("dd", e);
            }
        }
// ReSharper disable once RedundantCatchClause
        catch (Exception)
        {

            throw;
        }
    }

    //public int MethodRecursive()
    //{
    //    return MethodRecursive();
    //}
}