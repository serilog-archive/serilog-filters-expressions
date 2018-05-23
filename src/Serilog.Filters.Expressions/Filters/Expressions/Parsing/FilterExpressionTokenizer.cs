using Superpower;
using Superpower.Model;
using System.Collections.Generic;

namespace Serilog.Filters.Expressions.Parsing
{
    class FilterExpressionTokenizer : Tokenizer<FilterExpressionToken>
    {
        static readonly FilterExpressionToken[] SimpleOps = new FilterExpressionToken[128];

        static readonly HashSet<FilterExpressionToken> PreRegexTokens = new HashSet<FilterExpressionToken>
        {
            FilterExpressionToken.And,
            FilterExpressionToken.Or,
            FilterExpressionToken.Not,
            FilterExpressionToken.LParen,
            FilterExpressionToken.LBracket,
            FilterExpressionToken.Comma,
            FilterExpressionToken.Equal,
            FilterExpressionToken.NotEqual,
            FilterExpressionToken.Like,
            FilterExpressionToken.GreaterThan,
            FilterExpressionToken.GreaterThanOrEqual,
            FilterExpressionToken.LessThan,
            FilterExpressionToken.LessThanOrEqual,
            FilterExpressionToken.In,
            FilterExpressionToken.Is
        };

        static readonly FilterExpressionKeyword[] SqlKeywords =
        {
            new FilterExpressionKeyword("and", FilterExpressionToken.And),
            new FilterExpressionKeyword("in", FilterExpressionToken.In),
            new FilterExpressionKeyword("is", FilterExpressionToken.Is),
            new FilterExpressionKeyword("like", FilterExpressionToken.Like),
            new FilterExpressionKeyword("not", FilterExpressionToken.Not),
            new FilterExpressionKeyword("or", FilterExpressionToken.Or),
            new FilterExpressionKeyword("true", FilterExpressionToken.True),
            new FilterExpressionKeyword("false", FilterExpressionToken.False),
            new FilterExpressionKeyword("null", FilterExpressionToken.Null)
        };

        static FilterExpressionTokenizer()
        {
            SimpleOps['+'] = FilterExpressionToken.Plus;
            SimpleOps['-'] = FilterExpressionToken.Minus;
            SimpleOps['*'] = FilterExpressionToken.Asterisk;
            SimpleOps['/'] = FilterExpressionToken.ForwardSlash;
            SimpleOps['%'] = FilterExpressionToken.Percent;
            SimpleOps['^'] = FilterExpressionToken.Caret;
            SimpleOps['<'] = FilterExpressionToken.LessThan;
            SimpleOps['>'] = FilterExpressionToken.GreaterThan;
            SimpleOps['='] = FilterExpressionToken.Equal;
            SimpleOps[','] = FilterExpressionToken.Comma;
            SimpleOps['.'] = FilterExpressionToken.Period;
            SimpleOps['('] = FilterExpressionToken.LParen;
            SimpleOps[')'] = FilterExpressionToken.RParen;
            SimpleOps['['] = FilterExpressionToken.LBracket;
            SimpleOps[']'] = FilterExpressionToken.RBracket;
            SimpleOps['*'] = FilterExpressionToken.Asterisk;
            SimpleOps['?'] = FilterExpressionToken.QuestionMark;
        }

        protected override IEnumerable<Result<FilterExpressionToken>> Tokenize(
            TextSpan stringSpan,
            TokenizationState<FilterExpressionToken> tokenizationState)
        {
            var next = SkipWhiteSpace(stringSpan);
            if (!next.HasValue)
                yield break;

            do
            {
                if (char.IsDigit(next.Value))
                {
                    var hex = FilterExpressionTextParsers.HexInteger(next.Location);
                    if (hex.HasValue)
                    {
                        next = hex.Remainder.ConsumeChar();
                        yield return Result.Value(FilterExpressionToken.HexNumber, hex.Location, hex.Remainder);
                    }
                    else
                    {
                        var real = FilterExpressionTextParsers.Real(next.Location);
                        if (!real.HasValue)
                            yield return Result.CastEmpty<TextSpan, FilterExpressionToken>(real);
                        else
                            yield return Result.Value(FilterExpressionToken.Number, real.Location, real.Remainder);

                        next = real.Remainder.ConsumeChar();
                    }

                    if (next.HasValue && !char.IsPunctuation(next.Value) && !char.IsWhiteSpace(next.Value))
                    {
                        yield return Result.Empty<FilterExpressionToken>(next.Location, new[] { "digit" });
                    }
                }
                else if (next.Value == '\'')
                {
                    var str = FilterExpressionTextParsers.SqlString(next.Location);
                    if (!str.HasValue)
                        yield return Result.CastEmpty<string, FilterExpressionToken>(str);

                    next = str.Remainder.ConsumeChar();

                    yield return Result.Value(FilterExpressionToken.String, str.Location, str.Remainder);
                }
                else if (next.Value == '@')
                {
                    var beginIdentifier = next.Location;
                    var startOfName = next.Remainder;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && char.IsLetterOrDigit(next.Value));

                    if (next.Remainder == startOfName)
                    {
                        yield return Result.Empty<FilterExpressionToken>(startOfName, new[] { "built-in identifier name" });
                    }
                    else
                    {
                        yield return Result.Value(FilterExpressionToken.BuiltInIdentifier, beginIdentifier, next.Location);
                    }
                }
                else if (char.IsLetter(next.Value) || next.Value == '_')
                {
                    var beginIdentifier = next.Location;
                    do
                    {
                        next = next.Remainder.ConsumeChar();
                    }
                    while (next.HasValue && (char.IsLetterOrDigit(next.Value) || next.Value == '_'));

                    FilterExpressionToken keyword;
                    if (TryGetKeyword(beginIdentifier.Until(next.Location), out keyword))
                    {
                        yield return Result.Value(keyword, beginIdentifier, next.Location);
                    }
                    else
                    {
                        yield return Result.Value(FilterExpressionToken.Identifier, beginIdentifier, next.Location);
                    }
                }
                else if (next.Value == '/' && 
                         (!tokenizationState.Previous.HasValue || 
                            PreRegexTokens.Contains(tokenizationState.Previous.Value.Kind)))
                {
                    var regex = FilterExpressionTextParsers.RegularExpression(next.Location);
                    if (!regex.HasValue)
                        yield return Result.CastEmpty<Unit, FilterExpressionToken>(regex);

                    yield return Result.Value(FilterExpressionToken.RegularExpression, next.Location, regex.Remainder);
                    next = regex.Remainder.ConsumeChar();
                }
                else
                {
                    var compoundOp = FilterExpressionTextParsers.CompoundOperator(next.Location);
                    if (compoundOp.HasValue)
                    {
                        yield return Result.Value(compoundOp.Value, compoundOp.Location, compoundOp.Remainder);
                        next = compoundOp.Remainder.ConsumeChar();
                    }
                    else if (next.Value < SimpleOps.Length && SimpleOps[next.Value] != FilterExpressionToken.None)
                    {
                        yield return Result.Value(SimpleOps[next.Value], next.Location, next.Remainder);
                        next = next.Remainder.ConsumeChar();
                    }
                    else
                    {
                        yield return Result.Empty<FilterExpressionToken>(next.Location);
                        next = next.Remainder.ConsumeChar();
                    }
                }

                next = SkipWhiteSpace(next.Location);
            } while (next.HasValue);
        }

        static bool TryGetKeyword(TextSpan span, out FilterExpressionToken keyword)
        {
            foreach (var kw in SqlKeywords)
            {
                if (span.EqualsValueIgnoreCase(kw.Text))
                {
                    keyword = kw.Token;
                    return true;
                }
            }

            keyword = FilterExpressionToken.None;
            return false;
        }
    }
}
