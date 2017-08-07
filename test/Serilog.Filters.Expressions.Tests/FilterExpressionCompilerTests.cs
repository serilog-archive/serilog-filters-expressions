using Serilog.Events;
using Serilog.Filters.Expressions.Tests.Support;
using System.Linq;
using Xunit;

namespace Serilog.Filters.Expressions.Tests
{
    public class FilterExpressionCompilerTests
    {
        [Fact]
        public void FilterExpressionsEvaluateStringEquality()
        {
            AssertFiltering("Fruit = 'Apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent(),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void ComparisonsAreCaseSensitive()
        {
            AssertFiltering("Fruit = 'Apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "APPLE"));
        }

        [Fact]
        public void FilterExpressionsEvaluateStringContent()
        {
            AssertFiltering("Fruit like '%pp%'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void FilterExpressionsEvaluateStringPrefix()
        {
            AssertFiltering("Fruit like 'Ap%'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void FilterExpressionsEvaluateStringSuffix()
        {
            AssertFiltering("Fruit like '%le'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void LikeIsCaseInsensitive()
        {
            AssertFiltering("Fruit like 'apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"));
        }

        [Fact]
        public void FilterExpressionsEvaluateNumericComparisons()
        {
            AssertFiltering("Volume > 11",
                Some.InformationEvent("Adding {Volume} L", 11.5),
                Some.InformationEvent("Adding {Volume} L", 11));
        }

        [Fact]
        public void FilterExpressionsEvaluateWildcardsOnCollectionItems()
        {
            AssertFiltering("Items[?] like 'C%'",
                Some.InformationEvent("Cart contains {@Items}", new[] { new[] { "Tea", "Coffee" } }), // Test helper doesn't correct this case
                Some.InformationEvent("Cart contains {@Items}", new[] { new[] { "Apricots" } }));
        }

        [Fact]
        public void FilterExpressionsEvaluateBuiltInProperties()
        {
            AssertFiltering("@Level = 'Information'",
                Some.InformationEvent(),
                Some.WarningEvent());
        }

        [Fact]
        public void FilterExpressionsEvaluateExistentials()
        {
            AssertFiltering("AppId is not null",
                Some.InformationEvent("{AppId}", 10),
                Some.InformationEvent("{AppId}", null),
                Some.InformationEvent());
        }

        [Fact]
        public void FilterExpressionsLogicalOperations()
        {
            AssertFiltering("A and B",
                Some.InformationEvent("{A} {B}", true, true),
                Some.InformationEvent("{A} {B}", true, false),
                Some.InformationEvent());
        }

        [Fact]
        public void FilterExpressionsEvaluateSubproperties()
        {
            AssertFiltering("Cart.Total > 10",
                Some.InformationEvent("Checking out {@Cart}", new { Total = 20 }),
                Some.InformationEvent("Checking out {@Cart}", new { Total = 5 }));
        }


        [Fact]
        public void SequenceLengthCanBeDetermined()
        {
            AssertFiltering("length(Items) > 1",
                Some.InformationEvent("Checking out {Items}", new object[] { new[] { "pears", "apples" }}),
                Some.InformationEvent("Checking out {Items}", new object[] { new[] { "pears" }}));
        }

        static void AssertFiltering(string expression, LogEvent match, params LogEvent[] noMatches)
        {
            var sink = new CollectingSink();

            var log = new LoggerConfiguration()
                .Filter.ByIncludingOnly(expression)
                .WriteTo.Sink(sink)
                .CreateLogger();

            foreach (var noMatch in noMatches)
                log.Write(noMatch);

            log.Write(match);

            Assert.Equal(1, sink.Events.Count);
            Assert.Same(match, sink.Events.Single());
        }
    }
}
