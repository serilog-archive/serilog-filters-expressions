using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;
using Serilog.Filters.Expressions.Runtime;
using Superpower;
using System.Linq;
using System.Text.RegularExpressions;

namespace Serilog.Filters.Expressions.Compilation.Like
{
    class LikeOperatorTransformer : FilterExpressionIdentityTransformer
    {
        public static FilterExpression Rewrite(FilterExpression expression)
        {
            return new LikeOperatorTransformer().Transform(expression);
        }

        static FilterExpression Like(FilterExpression lhs, FilterTextExpression rhs)
        {
            if (!rhs.Text.Contains('%') && !rhs.Text.Contains('_'))
                return new FilterCallExpression(Operators.OpEqual, lhs, new FilterTextExpression(rhs.Text, FilterTextMatching.Insensitive));

            if (rhs.Text.Length > 1 && !rhs.Text.Contains('_'))
            {
                var count = rhs.Text.Count(ch => ch == '%');

                if (count == 1)
                {
                    var idx = rhs.Text.IndexOf('%');
                    var rest = rhs.Text.Remove(idx, 1);

                    if (idx == 0)
                        return new FilterCallExpression(Operators.OpEndsWith, lhs,
                            new FilterTextExpression(rest, FilterTextMatching.Insensitive));

                    if (idx == rhs.Text.Length - 1)
                        return new FilterCallExpression(Operators.OpStartsWith, lhs,
                            new FilterTextExpression(rest, FilterTextMatching.Insensitive));
                }
                else if (count == 2 && rhs.Text.Length > 2)
                {
                    if (rhs.Text.StartsWith("%") && rhs.Text.EndsWith("%"))
                        return new FilterCallExpression(Operators.OpContains, lhs,
                            new FilterTextExpression(rhs.Text.Substring(1, rhs.Text.Length - 2), FilterTextMatching.Insensitive));
                }
            }

            var regex = "";
            foreach (var ch in rhs.Text)
            {
                if (ch == '%')
                    regex += "(.|\\r|\\n)*"; // ~= RegexOptions.Singleline
                else if (ch == '_')
                    regex += '.';
                else
                    regex += Regex.Escape(ch.ToString());
            }

            return new FilterCallExpression(Operators.OpEqual, lhs, new FilterTextExpression(regex, FilterTextMatching.RegularExpressionInsensitive));
        }

        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            var newOperands = lx.Operands.Select(Transform).ToArray();

            if (Operators.SameOperator(lx.OperatorName, Operators.IntermediateOpSqlLike))
            {
                if (newOperands.Length == 2 &&
                    newOperands[1] is FilterTextExpression)
                {
                    return Like(newOperands[0], (FilterTextExpression)newOperands[1]);
                }

                return new FilterConstantExpression(Undefined.Value);
            }

            if (Operators.SameOperator(lx.OperatorName, Operators.IntermediateOpSqlNotLike))
            {
                if (newOperands.Length == 2 &&
                    newOperands[1] is FilterTextExpression)
                {
                    return new FilterCallExpression(Operators.RuntimeOpStrictNot,
                        Like(newOperands[0], (FilterTextExpression)newOperands[1]));
                }

                return new FilterConstantExpression(Undefined.Value);
            }

            return new FilterCallExpression(lx.OperatorName, newOperands);
        }
    }
}
