using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace MethodTimer
{
    /// <summary>
    /// Used to flag items as requiring timing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Module |
                    AttributeTargets.Class |
                    AttributeTargets.Method |
                    AttributeTargets.Constructor)]
    public class TimeAttribute :
        Attribute
    {
        public TimeAttribute()
        {
        }

        public TimeAttribute(string format)
        {
        }
    }
}