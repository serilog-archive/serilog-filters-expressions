using Serilog.Filters.Expressions.Ast;
using Superpower;
using Superpower.Parsers;
using System;
using System.Globalization;
using System.Linq;

namespace Serilog.Filters.Expressions.Parsing
{
    static class FilterExpressionTokenParsers
    {
        static readonly TokenListParser<FilterExpressionToken, string> Add = Token.EqualTo(FilterExpressionToken.Plus).Value(Operators.OpAdd);
        static readonly TokenListParser<FilterExpressionToken, string> Subtract = Token.EqualTo(FilterExpressionToken.Minus).Value(Operators.OpSubtract);
        static readonly TokenListParser<FilterExpressionToken, string> Multiply = Token.EqualTo(FilterExpressionToken.Asterisk).Value(Operators.OpMultiply);
        static readonly TokenListParser<FilterExpressionToken, string> Divide = Token.EqualTo(FilterExpressionToken.ForwardSlash).Value(Operators.OpDivide);
        static readonly TokenListParser<FilterExpressionToken, string> Modulo = Token.EqualTo(FilterExpressionToken.Percent).Value(Operators.OpModulo);
        static readonly TokenListParser<FilterExpressionToken, string> Power = Token.EqualTo(FilterExpressionToken.Caret).Value(Operators.OpPower);
        static readonly TokenListParser<FilterExpressionToken, string> And = Token.EqualTo(FilterExpressionToken.And).Value(Operators.OpAnd);
        static readonly TokenListParser<FilterExpressionToken, string> Or = Token.EqualTo(FilterExpressionToken.Or).Value(Operators.OpOr);
        static readonly TokenListParser<FilterExpressionToken, string> Lte = Token.EqualTo(FilterExpressionToken.LessThanOrEqual).Value(Operators.OpLessThanOrEqual);
        static readonly TokenListParser<FilterExpressionToken, string> Lt = Token.EqualTo(FilterExpressionToken.LessThan).Value(Operators.OpLessThan);
        static readonly TokenListParser<FilterExpressionToken, string> Gt = Token.EqualTo(FilterExpressionToken.GreaterThan).Value(Operators.OpGreaterThan);
        static readonly TokenListParser<FilterExpressionToken, string> Gte = Token.EqualTo(FilterExpressionToken.GreaterThanOrEqual).Value(Operators.OpGreaterThanOrEqual);
        static readonly TokenListParser<FilterExpressionToken, string> Eq = Token.EqualTo(FilterExpressionToken.Equal).Value(Operators.OpEqual);
        static readonly TokenListParser<FilterExpressionToken, string> Neq = Token.EqualTo(FilterExpressionToken.NotEqual).Value(Operators.OpNotEqual);
        static readonly TokenListParser<FilterExpressionToken, string> Negate = Token.EqualTo(FilterExpressionToken.Minus).Value(Operators.OpNegate);
        static readonly TokenListParser<FilterExpressionToken, string> Not = Token.EqualTo(FilterExpressionToken.Not).Value(Operators.OpNot);
        static readonly TokenListParser<FilterExpressionToken, string> Like = Token.EqualTo(FilterExpressionToken.Like).Value(Operators.IntermediateOpSqlLike);
        static readonly TokenListParser<FilterExpressionToken, string> NotLike = Not.IgnoreThen(Like).Value(Operators.IntermediateOpSqlNotLike);
        static readonly TokenListParser<FilterExpressionToken, string> In = Token.EqualTo(FilterExpressionToken.In).Value(Operators.RuntimeOpSqlIn);
        static readonly TokenListParser<FilterExpressionToken, string> NotIn = Not.IgnoreThen(In).Value(Operators.IntermediateOpSqlNotIn);
        static readonly TokenListParser<FilterExpressionToken, string> Is = Token.EqualTo(FilterExpressionToken.Is).Value(Operators.IntermediateOpSqlIs);

        static readonly TokenListParser<FilterExpressionToken, Func<FilterExpression, FilterExpression>> PropertyPathStep =
            Token.EqualTo(FilterExpressionToken.Period)
                .IgnoreThen(Token.EqualTo(FilterExpressionToken.Identifier))
                .Then(n => Parse.Return<FilterExpressionToken, Func<FilterExpression, FilterExpression>>(r => new FilterSubpropertyExpression(n.ToStringValue(), r)));

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Wildcard =
            Token.EqualTo(FilterExpressionToken.QuestionMark).Value((FilterExpression)new FilterWildcardExpression(FilterWildcard.Any))
                .Or(Token.EqualTo(FilterExpressionToken.Asterisk).Value((FilterExpression)new FilterWildcardExpression(FilterWildcard.All)));

        static readonly TokenListParser<FilterExpressionToken, Func<FilterExpression, FilterExpression>> PropertyPathIndexerStep =
            from open in Token.EqualTo(FilterExpressionToken.LBracket)
            from indexer in Wildcard.Or(Parse.Ref(() => Expr))
            from close in Token.EqualTo(FilterExpressionToken.RBracket)
            select new Func<FilterExpression, FilterExpression>(r => new FilterCallExpression("ElementAt", r, indexer));

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Function =
            (from name in Token.EqualTo(FilterExpressionToken.Identifier)
            from lparen in Token.EqualTo(FilterExpressionToken.LParen)
                from expr in Parse.Ref(() => Expr).ManyDelimitedBy(Token.EqualTo(FilterExpressionToken.Comma))
             from rparen in Token.EqualTo(FilterExpressionToken.RParen)
            select (FilterExpression)new FilterCallExpression(name.ToStringValue(), expr)).Named("function");

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> ArrayLiteral =
        (from lbracket in Token.EqualTo(FilterExpressionToken.LBracket)
            from expr in Parse.Ref(() => Expr).ManyDelimitedBy(Token.EqualTo(FilterExpressionToken.Comma))
            from rbracket in Token.EqualTo(FilterExpressionToken.RBracket)
            select (FilterExpression)new FilterArrayExpression(expr)).Named("array");

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> RootProperty =
            Token.EqualTo(FilterExpressionToken.BuiltInIdentifier).Select(b => (FilterExpression)new FilterPropertyExpression(b.ToStringValue().Substring(1), true))
                .Or(Token.EqualTo(FilterExpressionToken.Identifier).Select(t => (FilterExpression)new FilterPropertyExpression(t.ToStringValue(), false)));

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> PropertyPath =
            (from notfunction in Parse.Not(Token.EqualTo(FilterExpressionToken.Identifier).IgnoreThen(Token.EqualTo(FilterExpressionToken.LParen)))
             from root in RootProperty
             from path in PropertyPathStep.Or(PropertyPathIndexerStep).Many()
             select path.Aggregate(root, (o, f) => f(o))).Named("property");

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> RegularExpression =
            Token.EqualTo(FilterExpressionToken.RegularExpression)
                .Select(r =>
                {
                    var value = r.ToStringValue();
                    return (FilterExpression)new FilterTextExpression(value.Substring(1, value.Length - 2), FilterTextMatching.RegularExpression);
                });

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> SqlString =
            Token.EqualTo(FilterExpressionToken.String)
                .Apply(FilterExpressionTextParsers.SqlString)
                .Select(s => (FilterExpression)new FilterTextExpression(s, FilterTextMatching.Exact));

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> HexNumber =
            Token.EqualTo(FilterExpressionToken.HexNumber)
                .Apply(FilterExpressionTextParsers.HexInteger)
                .SelectCatch(n => ulong.Parse(n, NumberStyles.HexNumber, CultureInfo.InvariantCulture), "the numeric literal is too large")
                .Select(u => (FilterExpression)new FilterConstantExpression((decimal)u));

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Number =
            Token.EqualTo(FilterExpressionToken.Number)
                .Apply(FilterExpressionTextParsers.Real)
                .SelectCatch(n => decimal.Parse(n.ToStringValue(), CultureInfo.InvariantCulture), "the numeric literal is too large")
                .Select(d => (FilterExpression)new FilterConstantExpression(d));

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Literal =
            SqlString
                .Or(RegularExpression)
                .Or(Number)
                .Or(HexNumber)
                .Or(Token.EqualTo(FilterExpressionToken.True).Value((FilterExpression)new FilterConstantExpression(true)))
                .Or(Token.EqualTo(FilterExpressionToken.False).Value((FilterExpression)new FilterConstantExpression(false)))
                .Or(Token.EqualTo(FilterExpressionToken.Null).Value((FilterExpression)new FilterConstantExpression(null)))
                .Named("literal");

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Item = Literal.Or(PropertyPath).Or(Function).Or(ArrayLiteral);

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Factor =
            (from lparen in Token.EqualTo(FilterExpressionToken.LParen)
             from expr in Parse.Ref(() => Expr)
             from rparen in Token.EqualTo(FilterExpressionToken.RParen)
             select expr)
                .Or(Item);

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Operand =
            (from op in Negate.Or(Not)
             from factor in Factor
             select MakeUnary(op, factor)).Or(Factor).Named("expression");

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> InnerTerm = Parse.Chain(Power, Operand, MakeBinary);

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Term = Parse.Chain(Multiply.Or(Divide).Or(Modulo), InnerTerm, MakeBinary);

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Comparand = Parse.Chain(Add.Or(Subtract), Term, MakeBinary);

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Comparison = Parse.Chain(Is.Or(NotLike.Try().Or(Like)).Or(NotIn.Try().Or(In)).Or(Lte.Or(Neq).Or(Lt)).Or(Gte.Or(Gt)).Or(Eq), Comparand, MakeBinary);

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Conjunction = Parse.Chain(And, Comparison, MakeBinary);

        static readonly TokenListParser<FilterExpressionToken, FilterExpression> Disjunction = Parse.Chain(Or, Conjunction, MakeBinary);

        public static readonly TokenListParser<FilterExpressionToken, FilterExpression> Expr = Disjunction;

        static FilterExpression MakeBinary(string operatorName, FilterExpression leftOperand, FilterExpression rightOperand)
        {
            return new FilterCallExpression(operatorName, leftOperand, rightOperand);
        }

        static FilterExpression MakeUnary(string operatorName, FilterExpression operand)
        {
            return new FilterCallExpression(operatorName, operand);
        }
    }
}
