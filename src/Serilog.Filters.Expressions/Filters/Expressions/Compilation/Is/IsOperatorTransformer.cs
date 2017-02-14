using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;

namespace Serilog.Filters.Expressions.Compilation.Is
{
    class IsOperatorTransformer : FilterExpressionIdentityTransformer
    {
        public static FilterExpression Rewrite(FilterExpression expression)
        {
            return new IsOperatorTransformer().Transform(expression);
        }

        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            if (!Operators.SameOperator(lx.OperatorName.ToLowerInvariant(), Operators.IntermediateOpSqlIs) || lx.Operands.Length != 2)
                return base.Transform(lx);

            var nul = lx.Operands[1] as FilterConstantExpression;
            if (nul != null)
            {
                if (nul.ConstantValue != null)
                    return base.Transform(lx);

                return new FilterCallExpression(Operators.RuntimeOpIsNull, lx.Operands[0]);
            }

            var not = lx.Operands[1] as FilterCallExpression;
            if (not == null || not.Operands.Length != 1)
                return base.Transform(lx);

            nul = not.Operands[0] as FilterConstantExpression;
            if (nul == null || nul.ConstantValue != null)
                return base.Transform(lx);

            return new FilterCallExpression(Operators.RuntimeOpIsNotNull, lx.Operands[0]);
        }
    }
}
