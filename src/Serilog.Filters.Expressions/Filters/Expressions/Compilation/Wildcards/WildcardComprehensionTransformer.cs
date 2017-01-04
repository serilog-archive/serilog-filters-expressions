using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;

namespace Serilog.Filters.Expressions.Compilation.Wildcards
{
    class WildcardComprehensionTransformer : FilterExpressionIdentityTransformer
    {
        int _nextParameter = 0;

        public static FilterExpression Expand(FilterExpression root)
        {
            var wc = new WildcardComprehensionTransformer();
            return wc.Transform(root);
        }

        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            if (!Operators.WildcardComparators.Contains(lx.OperatorName) || lx.Operands.Length != 2)
                return base.Transform(lx);

            var lhsIs = FilterExpressionWildcardSearch.FindElementAtWildcard(lx.Operands[0]);
            var rhsIs = FilterExpressionWildcardSearch.FindElementAtWildcard(lx.Operands[1]);
            if (lhsIs != null && rhsIs != null || lhsIs == null && rhsIs == null)
                return base.Transform(lx); // N/A, or invalid

            var wcpath = lhsIs != null ? lx.Operands[0] : lx.Operands[1];
            var comparand = lhsIs != null ? lx.Operands[1] : lx.Operands[0];
            var elmat = lhsIs ?? rhsIs;

            var parm = new FilterParameterExpression("p" + _nextParameter++);
            var nestedComparand = FilterExpressionNodeReplacer.Replace(wcpath, elmat, parm);

            var coll = elmat.Operands[0];
            var wc = ((FilterWildcardExpression)elmat.Operands[1]).Wildcard;

            var comparisonArgs = lhsIs != null ? new[] { nestedComparand, comparand } : new[] { comparand, nestedComparand };
            var body = new FilterCallExpression(lx.OperatorName, comparisonArgs);
            var lambda = new FilterLambdaExpression(new[] { parm }, body);

            var op = Operators.ToRuntimeWildcardOperator(wc);
            var call = new FilterCallExpression(op, new[] { coll, lambda });
            return Transform(call);
        }
    }
}
