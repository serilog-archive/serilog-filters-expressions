using Serilog.Events;
using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Costing;
using Serilog.Filters.Expressions.Compilation.Is;
using Serilog.Filters.Expressions.Compilation.Like;
using Serilog.Filters.Expressions.Compilation.Linq;
using Serilog.Filters.Expressions.Compilation.Text;
using Serilog.Filters.Expressions.Compilation.Wildcards;
using Serilog.Filters.Expressions.Compilation.Properties;
using Serilog.Serilog.Filters.Expressions.Runtime;
using System;

namespace Serilog.Filters.Expressions.Compilation
{
    static class FilterExpressionCompiler
    {
        public static Func<LogEvent, object> CompileAndExpose(FilterExpression expression)
        {
            var actual = expression;
            actual = PropertiesObjectAccessorTransformer.Rewrite(actual);
            actual = WildcardComprehensionTransformer.Expand(actual);
            actual = LikeOperatorTransformer.Rewrite(actual);
            actual = IsOperatorTransformer.Rewrite(actual);
            actual = FilterExpressionCostReordering.Reorder(actual);
            actual = TextMatchingTransformer.Rewrite(actual);

            var compiled = LinqExpressionCompiler.Compile(actual);
            return ctx =>
            {
                var result = compiled(ctx);
                return Representation.Expose(result);
            };
        }
    }
}
