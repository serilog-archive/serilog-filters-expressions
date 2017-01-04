using Serilog.Filters.Expressions.Ast;
using Superpower;
using System.Linq;

namespace Serilog.Filters.Expressions.Compilation.Text
{
    abstract class MatchingRewriter
    {
        public virtual FilterExpression Rewrite(FilterCallExpression lx,
            TextMatchingTransformer rewriteArm)
        {
            return new FilterCallExpression(lx.OperatorName, lx.Operands.Select(rewriteArm.Transform).ToArray());
        }

        protected static bool UseCaseInsensitiveRegexMatching(FilterTextExpression optTxt, FilterTextMatching matching)
        {
            return !(matching == FilterTextMatching.RegularExpression ||
                            optTxt?.Matching == FilterTextMatching.Exact);

        }

        protected static bool UseCaseInsensitiveTextMatching(FilterTextExpression tx0, FilterTextExpression tx1)
        {
            if (tx0 == null && tx1 == null)
                return false;

            return !(tx0?.Matching == FilterTextMatching.Exact || tx1?.Matching == FilterTextMatching.Exact);
        }
    }
}
