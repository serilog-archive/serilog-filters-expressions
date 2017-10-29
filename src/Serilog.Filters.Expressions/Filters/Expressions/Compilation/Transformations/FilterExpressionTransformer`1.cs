using Serilog.Filters.Expressions.Ast;
using System;

namespace Serilog.Filters.Expressions.Compilation.Transformations
{
    abstract class FilterExpressionTransformer<TResult>
    {
        public TResult Transform(FilterExpression expression)
        {
            switch (expression.Type)
            {
                case FilterExpressionType.Call:
                    return Transform((FilterCallExpression)expression);
                case FilterExpressionType.Constant:
                    return Transform((FilterConstantExpression)expression);
                case FilterExpressionType.Subproperty:
                    return Transform((FilterSubpropertyExpression)expression);
                case FilterExpressionType.Property:
                    return Transform((FilterPropertyExpression)expression);
                case FilterExpressionType.Text:
                    return Transform((FilterTextExpression)expression);
                case FilterExpressionType.Lambda:
                    return Transform((FilterLambdaExpression)expression);
                case FilterExpressionType.Parameter:
                    return Transform((FilterParameterExpression)expression);
                case FilterExpressionType.Wildcard:
                    return Transform((FilterWildcardExpression)expression);
                case FilterExpressionType.Array:
                    return Transform((FilterArrayExpression)expression);
                default:
                    throw new ArgumentException(expression.Type + " is not a valid expression type");
            }
        }

        protected abstract TResult Transform(FilterCallExpression lx);
        protected abstract TResult Transform(FilterConstantExpression cx);
        protected abstract TResult Transform(FilterPropertyExpression px);
        protected abstract TResult Transform(FilterSubpropertyExpression spx);
        protected abstract TResult Transform(FilterTextExpression tx);
        protected abstract TResult Transform(FilterLambdaExpression lmx);
        protected abstract TResult Transform(FilterParameterExpression prx);
        protected abstract TResult Transform(FilterWildcardExpression wx);
        protected abstract TResult Transform(FilterArrayExpression ax);
    }
}
