namespace Serilog.Filters.Expressions.Ast
{
    enum FilterExpressionType
    {
        None,
        Call,
        Constant,
        Property,
        Text,
        Subproperty,
        Wildcard,
        Parameter,
        Lambda,
        Array
    }
}
