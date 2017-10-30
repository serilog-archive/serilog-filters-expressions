using System;
using System.Linq;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterArrayExpression : FilterExpression
    {
        public FilterArrayExpression(FilterExpression[] elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public FilterExpression[] Elements { get; }

        public override string ToString()
        {
            return "[" + string.Join(",", Elements.Select(o => o.ToString())) + "]";
        }
    }
}
