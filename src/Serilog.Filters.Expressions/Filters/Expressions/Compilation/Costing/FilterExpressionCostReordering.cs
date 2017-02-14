using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;
using Superpower;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Filters.Expressions.Compilation.Costing
{
    class FilterExpressionCostReordering : FilterExpressionTransformer<FilterExpressionCosting>
    {
        public static FilterExpression Reorder(FilterExpression expression)
        {
            var costing = new FilterExpressionCostReordering();
            return costing.Transform(expression).Expression;
        }

        protected override FilterExpressionCosting Transform(FilterPropertyExpression px)
        {
            if (px.PropertyName == "Exception" && px.IsBuiltIn)
                return new FilterExpressionCosting(px, 100);

            if (px.PropertyName == "Level" && px.IsBuiltIn)
                return new FilterExpressionCosting(px, 5);

            if (px.IsBuiltIn)
                return new FilterExpressionCosting(px, 1);

            return new FilterExpressionCosting(px, 10);
        }

        protected override FilterExpressionCosting Transform(FilterTextExpression tx)
        {
            switch (tx.Matching)
            {
                case FilterTextMatching.Insensitive:
                    return new FilterExpressionCosting(tx, 2);
                case FilterTextMatching.RegularExpression:
                case FilterTextMatching.RegularExpressionInsensitive:
                    return new FilterExpressionCosting(tx, 10000);
            }

            return new FilterExpressionCosting(tx, 1);
        }

        protected override FilterExpressionCosting Transform(FilterParameterExpression prx)
        {
            return new FilterExpressionCosting(prx, 0.1);
        }

        protected override FilterExpressionCosting Transform(FilterWildcardExpression wx)
        {
            return new FilterExpressionCosting(wx, 0);
        }

        protected override FilterExpressionCosting Transform(FilterLambdaExpression lmx)
        {
            var body = Transform(lmx.Body);
            return new FilterExpressionCosting(
                new FilterLambdaExpression(lmx.Parameters, body.Expression),
                body.Costing + 0.1);
        }

        protected override FilterExpressionCosting Transform(FilterSubpropertyExpression spx)
        {
            var receiver = Transform(spx.Receiver);
            return new FilterExpressionCosting(
                new FilterSubpropertyExpression(spx.PropertyName, receiver.Expression),
                receiver.Costing + 0.1);
        }

        protected override FilterExpressionCosting Transform(FilterConstantExpression cx)
        {
            return new FilterExpressionCosting(cx, 0.001);
        }

        protected override FilterExpressionCosting Transform(FilterCallExpression lx)
        {
            var operands = lx.Operands.Select(Transform).ToArray();
            var operatorName = lx.OperatorName.ToLowerInvariant();

            // To-do: the literal operator name comparisons here can be replaced with constants and IsSameOperator().
            if ((operatorName == "and" || operatorName == "or") && operands.Length == 2)
            {
                var reorderable = new List<FilterExpressionCosting>();
                foreach (var operand in operands)
                {
                    if (operand.ReorderableOperator?.ToLowerInvariant() == operatorName)
                    {
                        foreach (var ro in operand.ReorderableOperands)
                            reorderable.Add(ro);
                    }
                    else
                    {
                        reorderable.Add(operand);
                    }
                }

                var remaining = new Stack<FilterExpressionCosting>(reorderable.OrderBy(r => r.Costing));
                var top = remaining.Pop();
                var rhsExpr = top.Expression;
                var rhsCosting = top.Costing;

                while (remaining.Count != 0)
                {
                    var lhs = remaining.Pop();
                    rhsExpr = new FilterCallExpression(lx.OperatorName, lhs.Expression, rhsExpr);
                    rhsCosting = lhs.Costing + 0.75 * rhsCosting;
                }

                return new FilterExpressionCosting(
                    rhsExpr,
                    rhsCosting,
                    lx.OperatorName,
                    reorderable.ToArray());
            }

            if ((operatorName == "any" || operatorName == "all") && operands.Length == 2)
            {
                return new FilterExpressionCosting(
                    new FilterCallExpression(lx.OperatorName, operands[0].Expression, operands[1].Expression),
                    operands[0].Costing + 0.1 + operands[1].Costing * 7);
            }

            if ((operatorName == "startswith" || operatorName == "endswith" ||
                operatorName == "contains" || operatorName == "indexof" ||
                operatorName == "equal" || operatorName == "notequal") && operands.Length == 2)
            {
                return new FilterExpressionCosting(
                    new FilterCallExpression(lx.OperatorName, operands[0].Expression, operands[1].Expression),
                    operands[0].Costing + operands[1].Costing + 10);
            }

            return new FilterExpressionCosting(
                new FilterCallExpression(lx.OperatorName, operands.Select(o => o.Expression).ToArray()),
                operands.Sum(o => o.Costing) + 0.1);
        }
    }
}
