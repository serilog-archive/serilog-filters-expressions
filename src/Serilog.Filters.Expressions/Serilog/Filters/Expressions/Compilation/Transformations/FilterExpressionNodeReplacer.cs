using Serilog.Filters.Expressions.Ast;

namespace Serilog.Filters.Expressions.Compilation.Transformations
{
    class FilterExpressionNodeReplacer : FilterExpressionIdentityTransformer
    {
        readonly FilterExpression _source;
        readonly FilterExpression _dest;

        public static FilterExpression Replace(FilterExpression expr, FilterExpression source, FilterExpression dest)
        {
            var replacer = new FilterExpressionNodeReplacer(source, dest);
            return replacer.Transform(expr);
        }

        FilterExpressionNodeReplacer(FilterExpression source, FilterExpression dest)
        {
            _source = source;
            _dest = dest;
        }

        protected override FilterExpression Transform(FilterCallExpression lx)
        {
            if (lx == _source)
                return _dest;

            return base.Transform(lx);
        }

        protected override FilterExpression Transform(FilterConstantExpression cx)
        {
            if (cx == _source)
                return _dest;

            return base.Transform(cx);
        }

        protected override FilterExpression Transform(FilterPropertyExpression px)
        {
            if (px == _source)
                return _dest;

            return base.Transform(px);
        }

        protected override FilterExpression Transform(FilterSubpropertyExpression spx)
        {
            if (spx == _source)
                return _dest;

            return base.Transform(spx);
        }

        protected override FilterExpression Transform(FilterTextExpression tx)
        {
            if (tx == _source)
                return _dest;

            return base.Transform(tx);
        }

        protected override FilterExpression Transform(FilterLambdaExpression lmx)
        {
            if (lmx == _source)
                return _dest;

            return base.Transform(lmx);
        }

        protected override FilterExpression Transform(FilterParameterExpression prx)
        {
            if (prx == _source)
                return _dest;

            return base.Transform(prx);
        }

        protected override FilterExpression Transform(FilterWildcardExpression wx)
        {
            if (wx == _source)
                return _dest;

            return base.Transform(wx);
        }
    }
}
