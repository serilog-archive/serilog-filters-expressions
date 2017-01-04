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
        /// <param name="filterExpression">The expression to apply.</param>
        /// <returns>The underlying <see cref="LoggerConfiguration"/>.</returns>
        public static LoggerConfiguration ByIncludingOnly(this LoggerFilterConfiguration loggerFilterConfiguration, string filterExpression)
        {
            if (loggerFilterConfiguration == null)
                throw new ArgumentNullException(nameof(loggerFilterConfiguration));

            if (filterExpression == null)
                throw new ArgumentNullException(nameof(filterExpression));

            var compiled = FilterLanguage.CreateFilter(filterExpression);
            return loggerFilterConfiguration.ByIncludingOnly(e => true.Equals(compiled(e)));
        }

        /// <summary>
        /// Exclude log events that match the provided expression.
        /// </summary>
        /// <param name="loggerFilterConfiguration">Filter configuration.</param>
        /// <param name="filterExpression">The expression to apply.</param>
        /// <returns>The underlying <see cref="LoggerConfiguration"/>.</returns>
        public static LoggerConfiguration ByExcluding(this LoggerFilterConfiguration loggerFilterConfiguration, string filterExpression)
        {
            if (loggerFilterConfiguration == null)
                throw new ArgumentNullException(nameof(loggerFilterConfiguration));

            if (filterExpression == null)
                throw new ArgumentNullException(nameof(filterExpression));

            var compiled = FilterLanguage.CreateFilter(filterExpression);
            return loggerFilterConfiguration.ByExcluding(e => true.Equals(compiled(e)));
        }
    }
}
