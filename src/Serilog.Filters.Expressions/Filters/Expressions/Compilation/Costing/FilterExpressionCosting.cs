using Serilog.Filters.Expressions.Ast;

namespace Serilog.Filters.Expressions.Compilation.Costing
{
    class FilterExpressionCosting
    {
        public FilterExpression Expression { get; }
        public double Costing { get; }
        public string ReorderableOperator { get; }
        public FilterExpressionCosting[] ReorderableOperands { get; }

        public FilterExpressionCosting(FilterExpression expression, double costing, string reorderableOperator = null, FilterExpressionCosting[] reorderableOperands = null)
        {
            Expression = expression;
            Costing = costing;
            ReorderableOperator = reorderableOperator;
            ReorderableOperands = reorderableOperands;
        }
    }
}
