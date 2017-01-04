using System;

namespace Serilog.Filters.Expressions.Parsing
{
    struct FilterExpressionKeyword
    {
        public string Text { get; }
        public FilterExpressionToken Token { get; }

        public FilterExpressionKeyword(string text, FilterExpressionToken token)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Text = text;
            Token = token;
        }
    }
}
