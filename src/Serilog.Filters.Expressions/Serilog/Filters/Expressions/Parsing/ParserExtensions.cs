using System;
using Superpower;
using Superpower.Model;

namespace Serilog.Filters.Expressions.Parsing
{
    static partial class ParserExtensions
    {
        public static TextParser<U> SelectCatch<T, U>(this TextParser<T> parser, Func<T, U> trySelector, string errorMessage)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (trySelector == null) throw new ArgumentNullException(nameof(trySelector));
            if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

            return input =>
            {
                var t = parser(input);
                if (!t.HasValue)
                    return Superpower.Model.Result.CastEmpty<T, U>(t);

                try
                {
                    var u = trySelector(t.Value);
                    return Superpower.Model.Result.Value(u, input, t.Remainder);
                }
                catch
                {
                    return Superpower.Model.Result.Empty<U>(input, errorMessage);
                }
            };
        }

        public static TokenListParser<TTokenKind, U> SelectCatch<TTokenKind, T, U>(this TokenListParser<TTokenKind, T> parser, Func<T, U> trySelector, string errorMessage)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (trySelector == null) throw new ArgumentNullException(nameof(trySelector));
            if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

            return input =>
            {
                var t = parser(input);
                if (!t.HasValue)
                    return TokenListParserResult.CastEmpty<TTokenKind, T, U>(t);

                try
                {
                    var u = trySelector(t.Value);
                    return TokenListParserResult.Value(u, input, t.Remainder);
                }
                catch
                {
                    return TokenListParserResult.Empty<TTokenKind, U>(input, errorMessage);
                }
            };
        }
    }
}