using System;

namespace Serilog.Filters.Expressions.Ast
{
    abstract class FilterExpression
    {
        readonly FilterExpressionType _type;

        protected FilterExpression()
        {
            var typeName = GetType().Name.Remove(0, 6).Replace("Expression", "");
            _type = (FilterExpressionType)Enum.Parse(typeof(FilterExpressionType), typeName);
        }

        public FilterExpressionType Type { get { return _type; } }
    }
}
