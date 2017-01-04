using Serilog.Filters.Expressions.Ast;
using System.Text.RegularExpressions;

namespace Serilog.Filters.Expressions.Compilation.Text
{
    class RegexSecondMatchCallRewriter : MatchingRewriter
    {
        public override FilterExpression Rewrite(FilterCallExpression lx, TextMatchingTransformer rewriteArm)
        {
            if (lx.Operands.Length != 2)
                return base.Rewrite(lx, rewriteArm);

            var tx0 = lx.Operands[0] as FilterTextExpression;
            var tx1 = lx.Operands[1] as FilterTextExpression;

            if (tx1?.Matching == FilterTextMatching.RegularExpression ||
                tx1?.Matching == FilterTextMatching.RegularExpressionInsensitive)
            {
                var ci = RegexOptions.None;
                if (UseCaseInsensitiveRegexMatching(tx0, tx1.Matching))
                    ci = RegexOptions.IgnoreCase;

                var refind = new Regex(tx1.Text, ci | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Multiline);
                return new FilterCallExpression(Operators.ToRuntimePattern(lx.OperatorName),
                    rewriteArm.Transform(lx.Operands[0]),
                    new FilterConstantExpression(refind));
            }

            return new FilterCallExpression(
                UseCaseInsensitiveTextMatching(tx0, tx1) ? Operators.ToRuntimeIgnoreCase(lx.OperatorName) : lx.OperatorName,
                rewriteArm.Transform(lx.Operands[0]),
                rewriteArm.Transform(lx.Operands[1]));
        }
    }
}
