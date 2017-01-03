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

        public static FilterExpression ParseExact(string query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return FilterExpressionTokenParsers.Expr.AtEnd().Parse(_tokenizer.Tokenize(query));
        }

        public static bool TryParse(string expression, out FilterExpression root, out string[] errors)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            var tokenList = _tokenizer.TryTokenize(expression);

            var errorList = new List<string>();
            if (!tokenList.HasValue)
            {
                errorList.Add(tokenList.ToString());
            }
            else
            {
                var result = FilterExpressionTokenParsers.Expr.AtEnd().TryParse(tokenList.Value);
                if (result.HasValue)
                {
                    root = result.Value;
                    errors = null;
                    return true;
                }

                errorList.Add(result.ToString());
            }

            root = null;
            errors = errorList.ToArray();
            return false;
        }
    }
}
