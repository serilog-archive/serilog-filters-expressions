using Serilog.Events;
using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Costing;
using Serilog.Filters.Expressions.Compilation.Is;
using Serilog.Filters.Expressions.Compilation.Like;
using Serilog.Filters.Expressions.Compilation.Linq;
using Serilog.Filters.Expressions.Compilation.Text;
using Serilog.Filters.Expressions.Compilation.Wildcards;
using Serilog.Filters.Expressions.CompilatProperties;
using Serilog.Filters.Expressions.Runtime;
using Superpower;
using System;
using System.Linq;

namespace Serilog.Filters.Expressions.Compilation
{
    static class FilterExpressionCompiler
    {
        public static CompiledFilterExpression CompileAndExpose(FilterExpression expression)
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
                return Expose(result);
            };
        }

        static object Expose(object internalValue)
        {
            if (internalValue is Undefined)
                return null;

            if (internalValue is ScalarValue)
                throw new InvalidOperationException("A `ScalarValue` should have been converted within the filtering function, but was returned as a result.");

            var sequence = internalValue as SequenceValue;
            if (sequence != null)
                return sequence.Elements.Select(Expose).ToArray();

            var structure = internalValue as StructureValue;
            if (structure != null)
            {
                var r = structure.Properties.ToDictionary(p => p.Name, p => Expose(p.Value));
                if (structure.TypeTag != null)
                    r["$type"] = structure.TypeTag;
                return r;
            }

            var dictionary = internalValue as DictionaryValue;
            if (dictionary != null)
            {
                return dictionary.Elements.ToDictionary(p => Expose(p.Key), p => Expose(p.Value));
            }

            return internalValue;
        }
    }
}
