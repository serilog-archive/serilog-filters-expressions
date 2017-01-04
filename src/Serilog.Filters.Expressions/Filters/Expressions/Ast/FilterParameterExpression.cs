namespace Serilog.Filters.Expressions.Ast
{
    class FilterParameterExpression : FilterExpression
    {
        readonly string _parameterName;

        public FilterParameterExpression(string parameterName)
        {
            _parameterName = parameterName;
        }

        public string ParameterName
        {
            get { return _parameterName; }
        }

        public override string ToString()
        {
            // There's no parseable syntax for this yet
            return "$$" + ParameterName;
        }
    }
}