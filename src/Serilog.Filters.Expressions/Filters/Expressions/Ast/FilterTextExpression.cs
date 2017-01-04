using System;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterTextExpression : FilterExpression
    {
        readonly string _text;
        readonly FilterTextMatching _matching;

        public FilterTextExpression(string text, FilterTextMatching matching)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            _text = text;
            _matching = matching;
        }

        public string Text
        {
            get { return _text; }
        }

        public FilterTextMatching Matching
        {
            get { return _matching; }
        }

        public override string ToString()
        {
            // If it's a regular expression, the string is already regex-escaped.
            if (Matching == FilterTextMatching.RegularExpression)
                return "/" + Text + "/";

            if (Matching == FilterTextMatching.RegularExpressionInsensitive)
                return "/" + Text + "/i";

            var mm = Matching == FilterTextMatching.Exact ? "@" : "";
            return mm + "\"" + Text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") + "\"";
        }
    }

}
