using Serilog.Events;
using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation;
using System;
using System.Linq;

namespace Serilog.Filters.Expressions
{
    /// <summary>
    /// Helper methods to assist with construction of well-formed filters.
    /// </summary>
    public static class FilterLanguage
    {
        /// <summary>
        /// Create a log event filter based on the provided expression.
        /// </summary>
        /// <param name="expression">A filter expression.</param>
        /// <returns>A function that evaluates the expression in the context of the log event.</returns>
        public static Func<LogEvent, object> CreateFilter(string expression)
        {
            Func<LogEvent, object> filter;
            string error;
            if (!TryCreateFilter(expression, out filter, out error))
                throw new ArgumentException(error);

            return filter;
        }

        /// <summary>
        /// Create a log event filter based on the provided expression.
        /// </summary>
        /// <param name="expression">A filter expression.</param>
        /// <param name="filter">A function that evaluates the expression in the context of the log event.</param>
        /// <param name="error">The reported error, if compilation was unsuccessful.</param>
        /// <returns>True if the filter could be created; otherwise, false.</returns>
        public static bool TryCreateFilter(string expression, out Func<LogEvent, object> filter, out string error)
        {
            FilterExpression root;
            if (!FilterExpressionParser.TryParse(expression, out root, out error))
            {
                filter = null;
                return false;
            }

            filter = FilterExpressionCompiler.CompileAndExpose(root);
            error = null;
            return true;
        }

        /// <summary>
        /// Escape a value that is to appear in a `like` expression.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The text with any special values escaped. Will need to be passed through
        /// <see cref="EscapeStringContent(string)"/> if it is being embedded directly into a filter expression.</returns>
        public static string EscapeLikeExpressionContent(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return EscapeStringContent(text)
                .Replace("%", "%%")
                .Replace("_", "__");
        }

        /// <summary>
        /// Escape a fragment of text that will appear within a string.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The text with any special values escaped.</returns>
        public static string EscapeStringContent(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return text.Replace("'", "''");
        }

        /// <summary>
        /// Determine if the specified text is a valid property name.
        /// </summary>
        /// <param name="propertyName">The text to check.</param>
        /// <returns>True if the text can be used verbatim as a property name.</returns>
        public static bool IsValidPropertyName(string propertyName)
        {
            return propertyName.Length != 0 &&
                   !char.IsDigit(propertyName[0]) &&
                   propertyName.All(ch => char.IsLetter(ch) || char.IsDigit(ch) || ch == '_');
        }
    }
}