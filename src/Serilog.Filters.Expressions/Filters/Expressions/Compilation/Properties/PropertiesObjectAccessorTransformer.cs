using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;

namespace Serilog.Filters.Expressions.Compilation.Properties
{
    class PropertiesObjectAccessorTransformer : FilterExpressionIdentityTransformer
    {
        public static FilterExpression Rewrite(FilterExpression actual)
        {
            return new PropertiesObjectAccessorTransformer().Transform(actual);
        }

        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            if (!Operators.SameOperator(lx.OperatorName, Operators.OpElementAt) || lx.Operands.Length != 2)
                return base.Transform(lx);

            var p = lx.Operands[0] as FilterPropertyExpression;
            var n = lx.Operands[1] as FilterTextExpression;

            if (p == null || n == null || !p.IsBuiltIn || p.PropertyName != "Properties" ||
                n.Matching == FilterTextMatching.RegularExpression || n.Matching == FilterTextMatching.RegularExpressionInsensitive)
                return base.Transform(lx);

            return new FilterPropertyExpression(n.Text, false);
        }
    }
}
