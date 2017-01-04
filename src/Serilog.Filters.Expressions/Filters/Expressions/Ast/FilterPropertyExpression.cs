using System;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterPropertyExpression : FilterExpression
    {
        readonly string _propertyName;
        readonly bool _isBuiltIn;
        readonly bool _requiresEscape;

        public FilterPropertyExpression(string propertyName, bool isBuiltIn)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            _propertyName = propertyName;
            _isBuiltIn = isBuiltIn;
            _requiresEscape = !FilterLanguage.IsValidPropertyName(propertyName);
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public bool IsBuiltIn
        {
            get { return _isBuiltIn; }
        }

        public override string ToString()
        {
            if (_requiresEscape)
                return $"@Properties['{FilterLanguage.EscapeStringContent(PropertyName)}']";

            return (_isBuiltIn ? "@" : "") + PropertyName;
        }
    }
}