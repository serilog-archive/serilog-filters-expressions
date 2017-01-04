using System.Linq;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterLambdaExpression : FilterExpression
    {
        readonly FilterParameterExpression[] _parameters;
        readonly FilterExpression _body;

        public FilterLambdaExpression(FilterParameterExpression[] parameters, FilterExpression body)
        {
            _parameters = parameters;
            _body = body;
        }

        public FilterParameterExpression[] Parameters
        {
            get { return _parameters; }
        }

        public FilterExpression Body
        {
            get { return _body; }
        }

        public override string ToString()
        {
            // There's no parseable syntax for this yet
            return "\\(" + string.Join(",", Parameters.Select(p => p.ToString())) + "){" + Body + "}";
        }
    }
}