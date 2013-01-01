using System;

namespace MethodTimer
{
    /// <summary>
    /// Used to flag items as requiring timing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly |AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor,AllowMultiple = false)]
    public class TimeAttribute : Attribute
    {
    }
}