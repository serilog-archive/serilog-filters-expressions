using Serilog.Filters.Expressions.Ast;

namespace Serilog.Filters.Expressions.Compilation.Text
{
    class IndexOfIgnoreCaseCallRewriter : MatchingRewriter
    {
        public override FilterExpression Rewrite(FilterCallExpression lx, TextMatchingTransformer rewriteArm)
        {
            if (lx.Operands.Length != 2)
                return base.Rewrite(lx, rewriteArm);

            // Just a rename to match the runtime function.
            return new FilterCallExpression(
                Operators.RuntimeOpIndexOfIgnoreCase,
                rewriteArm.Transform(lx.Operands[0]),
                rewriteArm.Transform(lx.Operands[1]));
        }
    }
}
