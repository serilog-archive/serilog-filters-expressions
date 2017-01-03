using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterConstantExpression : FilterExpression
    {
        readonly object _constantValue;

        public FilterConstantExpression(object constantValue)
        {
            _constantValue = constantValue;
        }

        public object ConstantValue
        {
            get { return _constantValue; }
        }

        // Syntax here must match query syntax
        // or filters won't round-trip.
        //
        public override string ToString()
        {
            var s = ConstantValue as string;
            if (s != null)
            {
                return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            }

            var re = ConstantValue as Regex;
            if (re != null)
            {
                if ((re.Options & RegexOptions.IgnoreCase) != RegexOptions.None)
                    return $"/{re}/i";

                return $"/{re}/";
            }

            if (true.Equals(ConstantValue))
                return "true";
            
            if (false.Equals(ConstantValue))
                return "false";
            
            var formattable = _constantValue as IFormattable;
            if (formattable != null)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return (ConstantValue ?? "null").ToString();
        }
    }
}