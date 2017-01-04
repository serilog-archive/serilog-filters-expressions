using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Serilog.Events;
using Serilog.Filters.Expressions.PerformanceTests.Support;

namespace Serilog.Filters.Expressions.PerformanceTests
{
    /// <summary>
    /// Tests the overhead of determining the active logging level.
    /// </summary>
    public class ComparisonBenchmark
    {
        ILogger _trivialFilter, _handwrittenFilter, _expressionFilter;
        readonly LogEvent _event = Some.InformationEvent("{A}", 3);

        [Setup]
        public void Setup()
        {
            _trivialFilter = new LoggerConfiguration()
                .Filter.ByIncludingOnly(evt => true)
                .WriteTo.Sink(new NullSink())
                .CreateLogger();

            _handwrittenFilter = new LoggerConfiguration()
                .Filter.ByIncludingOnly(evt =>
                {
                    LogEventPropertyValue a;
                    if (!(evt.Properties.TryGetValue("A", out a) &&
                          a is ScalarValue &&
                          ((ScalarValue)a).Value is int))
                    {
                        return false;
                    }

                    return (int)((ScalarValue)a).Value == 3;
                })
                .WriteTo.Sink(new NullSink())
                .CreateLogger();

            _expressionFilter = new LoggerConfiguration()
                .Filter.ByIncludingOnly("A = 3")
                .WriteTo.Sink(new NullSink())
                .CreateLogger();
        }

        [Benchmark]
        public void TrivialFilter()
        {
            _trivialFilter.Write(_event);
        }  
        
        [Benchmark(Baseline = true)]
        public void HandwrittenFilter()
        {
            _handwrittenFilter.Write(_event);
        } 
                
        [Benchmark]
        public void ExpressionFilter()
        {
            _expressionFilter.Write(_event);
        }
    }
}
  