using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;
using Superpower;
using System.Linq;

namespace Serilog.Filters.Expressions.Compilation.Wildcards
{
    class FilterExpressionWildcardSearch : FilterExpressionTransformer<FilterCallExpression>
    {
        public static FilterCallExpression FindElementAtWildcard(FilterExpression fx)
        {
            var search = new FilterExpressionWildcardSearch();
            return search.Transform(fx);
        }

        protected override FilterCallExpression Transform(FilterCallExpression lx)
        {
            if (!Operators.SameOperator(lx.OperatorName, Operators.OpElementAt) || lx.Operands.Length != 2)
                return lx.Operands.Select(Transform).FirstOrDefault(e => e != null);

            if (lx.Operands[1] is FilterWildcardExpression)
                return lx;

            return Transform(lx.Operands[0]);
        }

        protected override FilterCallExpression Transform(FilterConstantExpression cx)
        {
            return null;
        }
        
        protected override FilterCallExpression Transform(FilterPropertyExpression px)
        {
            return null;
        }

        protected override FilterCallExpression Transform(FilterSubpropertyExpression spx)
        {
            return Transform(spx.Receiver);
        }

        protected override FilterCallExpression Transform(FilterTextExpression tx)
        {
            return null;
        }

        protected override FilterCallExpression Transform(FilterLambdaExpression lmx)
        {
            return null;
        }

        protected override FilterCallExpression Transform(FilterParameterExpression prx)
        {
            return null;
        }

        protected override FilterCallExpression Transform(FilterWildcardExpression wx)
        {
            // Must be RHS of ElementAt()
            return null;
        }
    }
}
