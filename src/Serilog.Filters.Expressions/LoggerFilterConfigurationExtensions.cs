using Serilog.Configuration;
using Serilog.Filters.Expressions;
using System;

namespace Serilog
{
    /// <summary>
    /// Extends logger configuration with methods for filtering with expressions.
    /// </summary>
    public static class LoggerFilterConfigurationExtensions
    {
        /// <summary>
        /// Include only log events that match the provided expression.
        /// </summary>
        /// <param name="loggerFilterConfiguration">Filter configuration.</param>
        /// <param name="expression">The expression to apply.</param>
        /// <returns>The underlying <see cref="LoggerConfiguration"/>.</returns>
        public static LoggerConfiguration ByIncludingOnly(this LoggerFilterConfiguration loggerFilterConfiguration, string expression)
        {
            if (loggerFilterConfiguration == null) throw new ArgumentNullException(nameof(loggerFilterConfiguration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var compiled = FilterLanguage.CreateFilter(expression);
            return loggerFilterConfiguration.ByIncludingOnly(e => true.Equals(compiled(e)));
        }

        /// <summary>
        /// Exclude log events that match the provided expression.
        /// </summary>
        /// <param name="loggerFilterConfiguration">Filter configuration.</param>
        /// <param name="expression">The expression to apply.</param>
        /// <returns>The underlying <see cref="LoggerConfiguration"/>.</returns>
        public static LoggerConfiguration ByExcluding(this LoggerFilterConfiguration loggerFilterConfiguration, string expression)
        {
            if (loggerFilterConfiguration == null) throw new ArgumentNullException(nameof(loggerFilterConfiguration));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var compiled = FilterLanguage.CreateFilter(expression);
            return loggerFilterConfiguration.ByExcluding(e => true.Equals(compiled(e)));
        }
    }
}
