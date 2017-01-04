using Serilog.Events;
using Serilog.Filters.Expressions.Ast;
using Serilog.Filters.Expressions.Compilation.Transformations;
using Serilog.Filters.Expressions.Runtime;
using Serilog.Serilog.Filters.Expressions.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Serilog.Filters.Expressions.Compilation.Linq
{
    class LinqExpressionCompiler : FilterExpressionTransformer<Expression<CompiledFilterExpression>>
    {
        static readonly IDictionary<string, MethodInfo> OperatorMethods = typeof(RuntimeOperators)
            .GetTypeInfo()
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);

        public static CompiledFilterExpression Compile(FilterExpression expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var compiler = new LinqExpressionCompiler();
            return compiler.Transform(expression).Compile();
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterCallExpression lx)
        {
            MethodInfo m;
            if (!OperatorMethods.TryGetValue(lx.OperatorName, out m))
                throw new ArgumentException($"The function name `{lx.OperatorName}` was not recognised; to search for text instead, enclose the filter in \"double quotes\".");

            if (m.GetParameters().Length != lx.Operands.Length)
                throw new ArgumentException($"The function `{lx.OperatorName}` requires {m.GetParameters().Length} arguments; to search for text instead, enclose the filter in \"double quotes\".");

            var acceptUndefined = m.GetCustomAttribute<AcceptUndefinedAttribute>() != null;
            var acceptNull = m.GetCustomAttribute<AcceptNullAttribute>() != null;
            var numericOnly = m.GetCustomAttribute<NumericAttribute>() != null;
            var numericComparable = m.GetCustomAttribute<NumericComparableAttribute>() != null;
            var booleanOnly = m.GetCustomAttribute<BooleanAttribute>() != null;
            var operands = lx.Operands.Select(Transform).ToArray();

            var returnUndefined = new List<Expression<Func<object, bool>>>();
            if (!acceptUndefined) returnUndefined.Add(v => v is Undefined);
            if (!acceptNull) returnUndefined.Add(v => v == null);
            if (numericOnly) returnUndefined.Add(v => !(v is decimal || v == null || v is Undefined));
            if (numericComparable) returnUndefined.Add(v => !(v is decimal || v == null || v is Undefined));
            if (booleanOnly) returnUndefined.Add(v => !(v is bool || v == null || v is Undefined));

            var context = Expression.Parameter(typeof(LogEvent));

            var operandValues = operands.Select(o => Splice(o, context));
            var operandVars = new List<ParameterExpression>();
            var rtn = Expression.Label(typeof(object));

            var statements = new List<Expression>();
            var first = true;
            foreach (var op in operandValues)
            {
                var opam = Expression.Variable(typeof(object));
                operandVars.Add(opam);
                statements.Add(Expression.Assign(opam, op));

                if (first && lx.OperatorName.ToLowerInvariant() == "and")
                {
                    Expression<Func<object, bool>> shortCircuitIf = v => !true.Equals(v);
                    var scc = Splice(shortCircuitIf, opam);
                    statements.Add(Expression.IfThen(scc, Expression.Return(rtn, Expression.Constant(false, typeof(object)))));
                }

                if (first && lx.OperatorName.ToLowerInvariant() == "or")
                {
                    Expression<Func<object, bool>> shortCircuitIf = v => true.Equals(v);
                    var scc = Splice(shortCircuitIf, opam);
                    statements.Add(Expression.IfThen(scc, Expression.Return(rtn, Expression.Constant(true, typeof(object)))));
                }

                var checks = returnUndefined.Select(fv => Splice(fv, opam)).ToArray();
                foreach (var check in checks)
                {
                    statements.Add(Expression.IfThen(check, Expression.Return(rtn, Expression.Constant(Undefined.Value, typeof(object)))));
                }

                first = false;
            }

            statements.Add(Expression.Return(rtn, Expression.Call(m, operandVars)));
            statements.Add(Expression.Label(rtn, Expression.Constant(Undefined.Value, typeof(object))));

            return Expression.Lambda<CompiledFilterExpression>(
                Expression.Block(typeof(object), operandVars, statements),
                context);
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterSubpropertyExpression spx)
        {
            var tgv = typeof(LinqExpressionCompiler).GetTypeInfo().GetMethod(nameof(TryGetStructurePropertyValue), BindingFlags.Static | BindingFlags.Public);
            var norm = typeof(Representation).GetTypeInfo().GetMethod(nameof(Representation.Represent), BindingFlags.Static | BindingFlags.Public);

            var recv = Transform(spx.Receiver);

            var context = Expression.Parameter(typeof(LogEvent));

            var r = Expression.Variable(typeof(object));
            var str = Expression.Variable(typeof(StructureValue));
            var result = Expression.Variable(typeof(LogEventPropertyValue));

            var sx3 = Expression.Call(tgv, str, Expression.Constant(spx.PropertyName, typeof(string)), result);

            var sx1 = Expression.Condition(sx3,
                            Expression.Call(norm, result),
                            Expression.Constant(Undefined.Value, typeof(object)));

            var sx2 = Expression.Block(typeof(object),
                    Expression.Assign(str, Expression.TypeAs(r, typeof(StructureValue))),
                    Expression.Condition(Expression.Equal(str, Expression.Constant(null, typeof(StructureValue))),
                        Expression.Constant(Undefined.Value, typeof(object)),
                        sx1));

            var assignR = Expression.Assign(r, Splice(recv, context));
            var getValue = Expression.Condition(Expression.TypeIs(r, typeof(Undefined)),
                Expression.Constant(Undefined.Value, typeof(object)),
                sx2);

            return Expression.Lambda<CompiledFilterExpression>(
                Expression.Block(typeof(object), new[] { r, str, result }, assignR, getValue),
                context);

            //return context =>
            //{
            //    var r = recv(context);
            //    if (r is Undefined)
            //        return Undefined.Value;

            //    var str = r as StructureValue;
            //    if (str == null)
            //        return Undefined.Value;

            //    LogEventPropertyValue result;
            //    if (!str.Properties.TryGetValue(spx.PropertyName, out result))
            //        return Undefined.Value;

            //    return Represent(result);
            //};
        }

        static Expression Splice(LambdaExpression lambda, params ParameterExpression[] newParameters)
        {
            var v = new ParameterReplacementVisitor(lambda.Parameters.ToArray(), newParameters);
            return v.Visit(lambda.Body);
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterConstantExpression cx)
        {
            return context => cx.ConstantValue;
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterPropertyExpression px)
        {
            if (px.IsBuiltIn)
            {
                if (px.PropertyName == "Level")
                    return context => context.Level.ToString() ?? "Information";

                if (px.PropertyName == "Message")
                    return context => NormalizeBaseDocumentProperty(context.RenderMessage(null));

                if (px.PropertyName == "Exception")
                    return context => NormalizeBaseDocumentProperty(context.Exception == null ? null : context.Exception.ToString());

                if (px.PropertyName == "Timestamp")
                    return context => context.Timestamp.ToString("o");

                if (px.PropertyName == "MessageTemplate")
                    return context => NormalizeBaseDocumentProperty(context.MessageTemplate.Text);

                if (px.PropertyName == "Properties")
                    return context => context.Properties;

                return context => Undefined.Value;
            }

            var propertyName = px.PropertyName;

            return context => GetPropertyValue(context, propertyName);
        }

        static object GetPropertyValue(LogEvent context, string propertyName)
        {
            LogEventPropertyValue value;
            if (!context.Properties.TryGetValue(propertyName, out value))
                return Undefined.Value;

            return Representation.Represent(value);
        }

        public static bool TryGetStructurePropertyValue(StructureValue sv, string name, out LogEventPropertyValue value)
        {
            foreach (var prop in sv.Properties)
            {
                if (prop.Name == name)
                {
                    value = prop.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        static object NormalizeBaseDocumentProperty(string rawValue)
        {
            // If a property like @Exception is null, it's not present at all, thus Undefined
            if (rawValue == null)
                return Undefined.Value;

            return rawValue;
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterTextExpression tx)
        {
            throw new InvalidOperationException("FilterTextExpression must be transformed prior to compilation.");
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterLambdaExpression lmx)
        {
            var context = Expression.Parameter(typeof(LogEvent));
            var parms = lmx.Parameters.Select(px => Tuple.Create(px, Expression.Parameter(typeof(object), px.ParameterName))).ToList();
            var body = Splice(Transform(lmx.Body), context);
            var paramSwitcher = new ExpressionConstantMapper(parms.ToDictionary(px => (object)px.Item1, px => (Expression)px.Item2));
            var rewritten = paramSwitcher.Visit(body);

            Type delegateType;
            if (lmx.Parameters.Length == 1)
                delegateType = typeof(Func<object, object>);
            else if (lmx.Parameters.Length == 2)
                delegateType = typeof(Func<object, object, object>);
            else
                throw new NotSupportedException("Unsupported lambda signature");

            var lambda = Expression.Lambda(delegateType, rewritten, parms.Select(px => px.Item2).ToArray());

            return Expression.Lambda<CompiledFilterExpression>(lambda, new[] { context });
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterParameterExpression prx)
        {
            // Will be within a lambda, which will subsequently sub-in the actual value
            var context = Expression.Parameter(typeof(LogEvent));
            var constant = Expression.Constant(prx, typeof(object));
            return Expression.Lambda<CompiledFilterExpression>(constant, new[] { context });
        }

        protected override Expression<CompiledFilterExpression> Transform(FilterWildcardExpression wx)
        {
            return context => Undefined.Value;
        }
    }
}
