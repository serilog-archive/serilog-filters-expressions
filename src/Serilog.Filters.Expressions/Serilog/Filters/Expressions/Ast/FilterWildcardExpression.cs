using System;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterWildcardExpression : FilterExpression
    {
        readonly FilterWildcard _wildcard;

        public FilterWildcardExpression(FilterWildcard wildcard)
        {
            _wildcard = wildcard;
        }

        public FilterWildcard Wildcard
        {
            get { return _wildcard; }
        }

        public override string ToString()
        {
            switch (Wildcard)
            {
                case FilterWildcard.Any:
                    return "?";
                case FilterWildcard.All:
                    return "*";
                default:
                    throw new NotSupportedException("Unrecognized wildcard " + Wildcard);
            }            
        }
    }
}