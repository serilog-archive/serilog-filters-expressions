using System;
using Serilog.Filters.Expressions.Ast;
using Superpower;

namespace Serilog.Filters.Expressions.Parsing
{
    static class FilterExpressionParser
    {
        static readonly FilterExpressionTokenizer Tokenizer = new FilterExpressionTokenizer();

        public static FilterExpression Parse(string filterExpression)
        {
            FilterExpression root;
            string error;
            if (!TryParse(filterExpression, out root, out error))
                throw new ArgumentException(error);

            return root;
        }

        public static bool TryParse(string filterExpression, out FilterExpression root, out string error)
        {
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            var tokenList = Tokenizer.TryTokenize(filterExpression);
            
            if (!tokenList.HasValue)
            {
                error = tokenList.ToString();
                root = null;
                return false;
            }

            var result = FilterExpressionTokenParsers.Expr.AtEnd().TryParse(tokenList.Value);
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
