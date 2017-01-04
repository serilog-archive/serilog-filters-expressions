using System;
using System.Linq;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterCallExpression : FilterExpression
    {
        readonly string _operatorName;
        readonly FilterExpression[] _operands;

        public FilterCallExpression(string operatorName, params FilterExpression[] operands)
        {
            if (operatorName == null) throw new ArgumentNullException(nameof(operatorName));
            if (operands == null) throw new ArgumentNullException(nameof(operands));
            _operatorName = operatorName;
            _operands = operands;
        }

        public string OperatorName
        {
            get { return _operatorName; }
        }

        public FilterExpression[] Operands
        {
            get { return _operands; }
        }

        public override string ToString()
        {
            if (OperatorName == "ElementAt" && Operands.Length == 2)
            {
                return Operands[0] + "[" + Operands[1] + "]";
            }

            return OperatorName + "(" + string.Join(",", Operands.Select(o => o.ToString())) + ")";
        }
    }
}