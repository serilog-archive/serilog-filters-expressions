using System.Collections.Generic;
using System.Linq.Expressions;

namespace Serilog.Filters.Expressions.Compilation.Linq
{
    class ExpressionConstantMapper : ExpressionVisitor
    {
        readonly IDictionary<object, Expression> _mapping;

        public ExpressionConstantMapper(IDictionary<object, Expression> mapping)
        {
            _mapping = mapping;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Expression substitute;
            if (node.Value != null && _mapping.TryGetValue(node.Value, out substitute))
                return substitute;

            return base.VisitConstant(node);
        }
    }
}
