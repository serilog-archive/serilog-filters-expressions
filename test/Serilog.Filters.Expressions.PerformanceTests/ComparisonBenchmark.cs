using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Serilog.Events;
using Serilog.Filters.Expressions.PerformanceTests.Support;
using System;
using Xunit;

namespace Serilog.Filters.Expressions.PerformanceTests
{
    /// <summary>
    /// Tests the performance of various filtering mechanisms.
    /// </summary>
    public class ComparisonBenchmark
    {
        Func<LogEvent, bool> _trivialFilter, _handwrittenFilter, _expressionFilter;
        readonly LogEvent _event = Some.InformationEvent("{A}", 3);

        [Setup]
        public void Setup()
        {
            // Just the delegate invocation overhead
            _trivialFilter = evt => true;

            // `A == 3`, the old way
            _handwrittenFilter = evt =>
            {
                LogEventPropertyValue a;
                if (evt.Properties.TryGetValue("A", out a) &&
                        a is ScalarValue &&
                        ((ScalarValue)a).Value is int)
                {
                    return (int)((ScalarValue)a).Value == 3;
                }

                return false;
            };

            // The code we're interested in; the `true.Equals()` overhead is normally added when
            // this is used with Serilog
            var compiled = FilterLanguage.CreateFilter("A = 3");
            _expressionFilter = evt => true.Equals(compiled(evt));

            Assert.True(_trivialFilter(_event) && _handwrittenFilter(_event) && _expressionFilter(_event));
        }

        [Benchmark]
        public void TrivialFilter()
        {
            _trivialFilter(_event);
        }  
        
        [Benchmark(Baseline = true)]
        public void HandwrittenFilter()
        {
            _handwrittenFilter(_event);
        } 
                
        [Benchmark]
        public void ExpressionFilter()
        {
            _expressionFilter(_event);
        }
    }
}
  