using Serilog.Events;

namespace Serilog.Filters.Expressions.Compilation
{
    delegate object CompiledFilterExpression(LogEvent context);
}
