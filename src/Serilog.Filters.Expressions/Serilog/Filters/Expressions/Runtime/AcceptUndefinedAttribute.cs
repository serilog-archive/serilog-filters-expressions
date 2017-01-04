using System;

namespace Serilog.Filters.Expressions.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    class AcceptUndefinedAttribute : Attribute
    {
    }
}
