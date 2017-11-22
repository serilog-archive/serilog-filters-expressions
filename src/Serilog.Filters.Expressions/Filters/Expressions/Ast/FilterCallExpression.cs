using System;
using System.Linq;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterCallExpression : FilterExpression
    {
        public FilterCallExpression(string operatorName, params FilterExpression[] operands)
        {
            OperatorName = operatorName ?? throw new ArgumentNullException(nameof(operatorName));
            Operands = operands ?? throw new ArgumentNullException(nameof(operands));
        }

        public string OperatorName { get; }

        public FilterExpression[] Operands { get; }

        public override string ToString()
        {
            if (OperatorName == Operators.OpElementAt && Operands.Length == 2)
            {
                return Operands[0] + "[" + Operands[1] + "]";
            }

            return OperatorName + "(" + string.Join(",", Operands.Select(o => o.ToString())) + ")";
        }
    }
}