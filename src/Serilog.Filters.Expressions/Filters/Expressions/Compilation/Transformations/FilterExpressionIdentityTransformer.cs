using Serilog.Filters.Expressions.Ast;
using Superpower;
using System.Linq;

namespace Serilog.Filters.Expressions.Compilation.Transformations
{
    class FilterExpressionIdentityTransformer : FilterExpressionTransformer<FilterExpression>
    {
        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            return new FilterCallExpression(lx.OperatorName, lx.Operands.Select(Transform).ToArray());
        }

        protected override FilterExpression Transform(FilterConstantExpression cx)
        {
            return cx;
        }

        protected override FilterExpression Transform(FilterPropertyExpression px)
        {
            return px;
        }

        protected override FilterExpression Transform(FilterSubpropertyExpression spx)
        {
            return new FilterSubpropertyExpression(spx.PropertyName, Transform(spx.Receiver));
        }

        protected override FilterExpression Transform(FilterTextExpression tx)
        {
            return tx;
        }

        protected override FilterExpression Transform(FilterLambdaExpression lmx)
        {
            // By default we maintain the parameters available in the body
            return new FilterLambdaExpression(lmx.Parameters, Transform(lmx.Body));
        }

        // Only touches uses of the parameters, not decls
        protected override FilterExpression Transform(FilterParameterExpression prx)
        {
            return prx;
        }

        protected override FilterExpression Transform(FilterWildcardExpression wx)
        {
            return wx;
        }

        protected override FilterExpression Transform(FilterArrayExpression ax)
        {
            return new FilterArrayExpression(ax.Elements.Select(Transform).ToArray());
        }
    }
}
