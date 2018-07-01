using System;
using Serilog.Filters.Expressions.Ast;

namespace Serilog.Filters.Expressions.Parsing
{
    static class FilterExpressionParser
    {
        public static FilterExpression Parse(string filterExpression)
        {
            if (!TryParse(filterExpression, out var root, out var error))
                throw new ArgumentException(error);

            return root;
        }

        public static bool TryParse(string filterExpression, out FilterExpression root, out string error)
        {
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            var tokenList = FilterExpressionTokenizer.Instance.TryTokenize(filterExpression);       
            if (!tokenList.HasValue)
            {
                error = tokenList.ToString();
                root = null;
                return false;
            }

            var result = FilterExpressionTokenParsers.TryParse(tokenList.Value);
            if (!result.HasValue)
            {
                error = result.ToString();
                root = null;
                return false;
            }

            root = result.Value;
            error = null;
            return true;
        }
    }
}
