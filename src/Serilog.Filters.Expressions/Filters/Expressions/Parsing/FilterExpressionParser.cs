using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Parsing;
using Superpower;
using System;
using System.Collections.Generic;

namespace Serilog.Filters.Expressions
{
    static class FilterExpressionParser
    {
        static readonly FilterExpressionTokenizer _tokenizer = new FilterExpressionTokenizer();

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

            var tokenList = _tokenizer.TryTokenize(filterExpression);

            var errorList = new List<string>();
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
