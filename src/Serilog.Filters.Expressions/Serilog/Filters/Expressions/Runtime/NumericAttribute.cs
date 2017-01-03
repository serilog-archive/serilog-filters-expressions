using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Serilog.Filters.Expressions.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    class NumericAttribute : Attribute
    {
    }
}
