using System.Linq;
using Serilog.Events;
using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;
using Serilog.Filters.Expressions.Runtime;

namespace Serilog.Filters.Expressions.Compilation.Arrays
{
    class FilterExpressionConstantArrayEvaluator : FilterExpressionIdentityTransformer
    {
        public static FilterExpression Evaluate(FilterExpression expression)
        {
            var evaluator = new FilterExpressionConstantArrayEvaluator();
            return evaluator.Transform(expression);
        }

        protected override FilterExpression Transform(FilterArrayExpression ax)
        {
            // This should probably go depth-first.

            if (ax.Elements.All(el => el is FilterConstantExpression))
            {
                return new FilterConstantExpression(
                    new SequenceValue(ax.Elements
                        .Cast<FilterConstantExpression>()
                        .Select(ce => Representation.Recapture(ce.ConstantValue))));
            }

            return base.Transform(ax);
        }
    }
}
