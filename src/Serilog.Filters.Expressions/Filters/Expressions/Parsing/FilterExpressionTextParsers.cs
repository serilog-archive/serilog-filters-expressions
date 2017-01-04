using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Linq;

namespace Serilog.Filters.Expressions.Parsing
{
    static class FilterExpressionTextParsers
    {
        static readonly TextParser<FilterExpressionToken> LessOrEqual = Span.EqualTo("<=").Value(FilterExpressionToken.LessThanOrEqual);
        static readonly TextParser<FilterExpressionToken> GreaterOrEqual = Span.EqualTo(">=").Value(FilterExpressionToken.GreaterThanOrEqual);
        static readonly TextParser<FilterExpressionToken> NotEqual = Span.EqualTo("<>").Value(FilterExpressionToken.NotEqual);

        public static TextParser<FilterExpressionToken> CompoundOperator = GreaterOrEqual.Or(LessOrEqual.Try().Or(NotEqual));

        public static TextParser<string> HexInteger =
            Span.EqualTo("0x")
                .IgnoreThen(Character.Digit.Or(Character.Matching(ch => ch >= 'a' && ch <= 'f' || ch >= 'A' && ch <= 'F', "a-f"))
                    .Named("hex digit")
                    .AtLeastOnce())
                .Select(chrs => new string(chrs));

        public static TextParser<char> SqlStringContentChar =
            Span.EqualTo("''").Value('\'').Try().Or(Character.ExceptIn('\'', '\r', '\n'));

        public static TextParser<string> SqlString =
            Character.EqualTo('\'')
                .IgnoreThen(SqlStringContentChar.Many())
                .Then(s => Character.EqualTo('\'').Value(new string(s)));

        public static TextParser<char> RegularExpressionContentChar { get; } =
            Span.EqualTo(@"\/").Value('/').Try().Or(Character.Except('/'));

        public static TextParser<Unit> RegularExpression =
            Character.EqualTo('/')
                .IgnoreThen(RegularExpressionContentChar.Many())
                .IgnoreThen(Character.EqualTo('/'))
                .Value(Unit.Value);

        public static TextParser<TextSpan> Real =
            Numerics.Integer
                .Then(n => Character.EqualTo('.').IgnoreThen(Numerics.Integer).OptionalOrDefault()
                    .Select(f => f == TextSpan.None ? n : new TextSpan(n.Source, n.Position, n.Length + f.Length + 1)));
    }
}
