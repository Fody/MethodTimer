using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MethodTimer
{

    /// <summary>
    /// Alternate attribute to test custom attributes used for timing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false)]
    public class AlternateAttribute : Attribute
    {
    }
}
