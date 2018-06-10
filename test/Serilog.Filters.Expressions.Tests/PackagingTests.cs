using System.Reflection;
using Xunit;

namespace Serilog.Filters.Expressions.Tests
{
    public class PackagingTests
    {
        [Fact]
        public void AssemblyVersionIsSet()
        {
            var version = typeof(LoggerFilterConfigurationExtensions).GetTypeInfo().Assembly.GetName().Version;
            Assert.Equal("2", version.ToString(1));
        }
    }
}
