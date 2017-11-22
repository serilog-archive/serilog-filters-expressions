using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;

namespace Serilog.Filters.Expressions.Compilation.In
{
    class FilterExpressionNotInRewriter : FilterExpressionIdentityTransformer
    {
        public static FilterExpression Rewrite(FilterExpression expression)
        {
            var rewriter = new FilterExpressionNotInRewriter();
            return rewriter.Transform(expression);
        }

        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            if (Operators.SameOperator(Operators.IntermediateOpSqlNotIn, lx.OperatorName))
                return new FilterCallExpression(Operators.RuntimeOpStrictNot,
                    new FilterCallExpression(Operators.RuntimeOpSqlIn, lx.Operands));
            return base.Transform(lx);
        }
    }
}
