using System;

namespace Serilog.Filters.Expressions.Ast
{
    class FilterSubpropertyExpression : FilterExpression
    {
        readonly string _propertyName;
        readonly FilterExpression _receiver;

        public FilterSubpropertyExpression(string propertyName, FilterExpression receiver)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            _propertyName = propertyName;
            _receiver = receiver;
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public FilterExpression Receiver
        {
            get { return _receiver; }
        }

        public override string ToString()
        {
            return _receiver + "." + PropertyName;
        }
    }
}
