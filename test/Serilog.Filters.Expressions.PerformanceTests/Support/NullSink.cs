using Serilog.Core;
using Serilog.Events;

namespace Serilog.Filters.Expressions.PerformanceTests.Support
{
    public class NullSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
        }
    }
}
