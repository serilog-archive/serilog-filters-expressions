using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;
using System.Collections.Generic;

namespace Serilog.Filters.Expressions.Compilation.Text
{
    class TextMatchingTransformer : FilterExpressionIdentityTransformer
    {
        static readonly Dictionary<string, MatchingRewriter> Rewriters = new Dictionary<string, MatchingRewriter>(Operators.OperatorComparer)
        {
            [Operators.OpContains] = new RegexSecondMatchCallRewriter(),
            [Operators.OpIndexOf] = new RegexSecondMatchCallRewriter(),
            [Operators.OpIndexOfIgnoreCase] = new IndexOfIgnoreCaseCallRewriter(),
            [Operators.OpStartsWith] = new RegexSecondMatchCallRewriter(),
            [Operators.OpEndsWith] = new RegexSecondMatchCallRewriter(),
            [Operators.OpEqual] = new SymmetricMatchCallRewriter(),
            [Operators.OpNotEqual] = new SymmetricMatchCallRewriter()
        };

        public static FilterExpression Rewrite(FilterExpression expression)
        {
            var transformer = new TextMatchingTransformer();
            return transformer.Transform(expression);
        }

        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            MatchingRewriter comparison;
            if (!Rewriters.TryGetValue(lx.OperatorName, out comparison))
                return base.Transform(lx);

            return comparison.Rewrite(lx, this);
        }

        protected override FilterExpression Transform(FilterTextExpression tx)
        {
            // Since at this point the value is not being used in any matching-compatible
            // operation, it doesn't matter what the matching mode is.
            return new FilterConstantExpression(tx.Text);
        }
    }
}
