using Serilog.Filters.Expressions.Ast;
using System.Text.RegularExpressions;

namespace Serilog.Filters.Expressions.Compilation.Text
{
    class SymmetricMatchCallRewriter : MatchingRewriter
    {
        public override FilterExpression Rewrite(FilterCallExpression lx, TextMatchingTransformer rewriteArm)
        {
            if (lx.Operands.Length != 2)
                return base.Rewrite(lx, rewriteArm);

            var tx0 = lx.Operands[0] as FilterTextExpression;
            var tx1 = lx.Operands[1] as FilterTextExpression;

            if (tx0?.Matching == FilterTextMatching.RegularExpression ||
                tx0?.Matching == FilterTextMatching.RegularExpressionInsensitive ||
                tx1?.Matching == FilterTextMatching.RegularExpression ||
                tx1?.Matching == FilterTextMatching.RegularExpressionInsensitive)
            {
                if (tx1?.Matching != FilterTextMatching.RegularExpression &&
                    tx1?.Matching != FilterTextMatching.RegularExpressionInsensitive)
                {
                    // Make sure the regex is always second.
                    return Rewrite(new FilterCallExpression(lx.OperatorName, lx.Operands[1], lx.Operands[0]), rewriteArm);
                }

                var ci = RegexOptions.None;
                if (UseCaseInsensitiveRegexMatching(tx0, tx1.Matching))
                    ci |= RegexOptions.IgnoreCase;

                var rewhole = new Regex("^" + tx1.Text + "$", ci | RegexOptions.ExplicitCapture);
                return new FilterCallExpression(Operators.ToRuntimePattern(lx.OperatorName),
                    rewriteArm.Transform(lx.Operands[0]),
                    new FilterConstantExpression(rewhole));
            }

            return new FilterCallExpression(
                UseCaseInsensitiveTextMatching(tx0, tx1) ? Operators.ToRuntimeIgnoreCase(lx.OperatorName) : lx.OperatorName,
                rewriteArm.Transform(lx.Operands[0]),
                rewriteArm.Transform(lx.Operands[1]));
        }
    }
}
