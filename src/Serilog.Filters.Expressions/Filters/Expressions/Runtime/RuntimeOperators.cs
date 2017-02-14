using Serilog.Events;
using Serilog.Filters.Expressions.Compilation.Linq;
using Serilog.Serilog.Filters.Expressions.Runtime;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Serilog.Filters.Expressions.Runtime
{
    static class RuntimeOperators
    {
        [Numeric]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Add(object left, object right)
        {
            return (decimal)left + (decimal)right;
        }

        [Numeric]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Subtract(object left, object right)
        {
            return (decimal)left - (decimal)right;
        }

        [Numeric]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Multiply(object left, object right)
        {
            return (decimal)left * (decimal)right;
        }

        [Numeric]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Divide(object left, object right)
        {
            if ((decimal)right == 0) return Undefined.Value;
            return (decimal)left / (decimal)right;
        }

        [Numeric]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Modulo(object left, object right)
        {
            if ((decimal)right == 0) return Undefined.Value;
            return (decimal)left % (decimal)right;
        }

        [Numeric]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Power(object left, object right)
        {
            return (decimal)Math.Pow((double)(decimal)left, (double)(decimal)right);
        }

        [Boolean]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object And(object left, object right)
        {
            return true.Equals(left) && true.Equals(right);
        }

        [Boolean, AcceptUndefined, AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Or(object left, object right)
        {
            return true.Equals(left) || true.Equals(right);
        }

        [NumericComparable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object LessThanOrEqual(object left, object right)
        {
            return ((decimal)left) <= ((decimal)right);
        }

        [NumericComparable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object LessThan(object left, object right)
        {
            return ((decimal)left) < ((decimal)right);
        }

        [NumericComparable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GreaterThan(object left, object right)
        {
            return ((decimal)left) > ((decimal)right);
        }

        [NumericComparable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GreaterThanOrEqual(object left, object right)
        {
            return ((decimal)left) >= ((decimal)right);
        }

        [AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Equal(object left, object right)
        {
            return left == null ? right == null : left.Equals(right);
        }

        [AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_EqualIgnoreCase(object left, object right)
        {
            if (left == null && right == null)
                return true;

            var ls = left as string;
            var rs = right as string;
            if (ls == null || rs == null)
                return Undefined.Value;

            return CompareInfo.Compare(ls, rs, CompareOptions.OrdinalIgnoreCase) == 0;
        }

        [AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_EqualPattern(object left, object right)
        {
            if (left == null && right == null)
                return true;

            var ls = left as string;
            var rs = right as Regex;
            if (ls == null || rs == null)
                return Undefined.Value;

            return rs.IsMatch(ls);
        }

        [AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object NotEqual(object left, object right)
        {
            return !(bool)Equal(left, right);
        }

        [AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_NotEqualIgnoreCase(object left, object right)
        {
            var r = _Internal_EqualIgnoreCase(left, right);
            if (ReferenceEquals(r, Undefined.Value)) return r;
            return !(bool)r;
        }

        [AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_NotEqualPattern(object left, object right)
        {
            var r = _Internal_EqualPattern(left, right);
            if (ReferenceEquals(r, Undefined.Value)) return r;
            return !(bool)r;
        }

        [Numeric]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Negate(object operand)
        {
            return -((decimal)operand);
        }

        [Boolean, AcceptUndefined]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Not(object operand)
        {
            if (operand is Undefined)
                return true;

            return !((bool)operand);
        }

        [Boolean]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_StrictNot(object operand)
        {
            return !((bool)operand);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Contains(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return ctx.Contains(ptx);
        }

        static readonly CompareInfo CompareInfo = CultureInfo.InvariantCulture.CompareInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_ContainsIgnoreCase(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return CompareInfo.IndexOf(ctx, ptx, CompareOptions.OrdinalIgnoreCase) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_ContainsPattern(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as Regex;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return ptx.IsMatch(ctx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object IndexOf(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return (decimal)ctx.IndexOf(ptx, StringComparison.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_IndexOfIgnoreCase(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return (decimal)ctx.IndexOf(ptx, StringComparison.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_IndexOfPattern(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as Regex;
            if (ctx == null || ptx == null)
                return Undefined.Value;

            var m = ptx.Match(ctx);
            if (!m.Success)
                return -1m;

            return (decimal)m.Index;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Length(object corpus)
        {
            var ctx = corpus as string;
            if (ctx == null)
                return Undefined.Value;
            return (decimal)ctx.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object StartsWith(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return ctx.StartsWith(ptx, StringComparison.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_StartsWithIgnoreCase(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return ctx.StartsWith(ptx, StringComparison.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_StartsWithPattern(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as Regex;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            var m = ptx.Match(ctx);
            return m.Success && m.Index == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object EndsWith(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return ctx.EndsWith(ptx, StringComparison.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_EndsWithIgnoreCase(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as string;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            return ctx.EndsWith(ptx, StringComparison.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_EndsWithPattern(object corpus, object pattern)
        {
            var ctx = corpus as string;
            var ptx = pattern as Regex;
            if (ctx == null || ptx == null)
                return Undefined.Value;
            var matches = ptx.Matches(ctx);
            if (matches.Count == 0)
                return false;
            var m = matches[matches.Count - 1];
            return m.Index + m.Length == ctx.Length;
        }

        [AcceptUndefined, AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Has(object value)
        {
            return !(value is Undefined);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ElementAt(object items, object index)
        {
            var arr = items as SequenceValue;
            if (arr != null)
            {
                if (!(index is decimal))
                    return Undefined.Value;

                var dec = (decimal)index;
                if (dec != Math.Floor(dec))
                    return Undefined.Value;

                var idx = (int)dec;
                if (idx >= arr.Elements.Count())
                    return Undefined.Value;

                return Representation.Represent(arr.Elements.ElementAt(idx));
            }

            var dict = items as StructureValue;
            if (dict != null)
            {
                var s = index as string;
                if (s == null)
                    return Undefined.Value;

                LogEventPropertyValue value;
                if (!LinqExpressionCompiler.TryGetStructurePropertyValue(dict, s, out value))
                    return Undefined.Value;

                return Representation.Represent(value);
            }

            return Undefined.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_Any(object items, object predicate)
        {
            var pred = predicate as Func<object, object>;
            if (pred == null)
                return Undefined.Value;

            SequenceValue arr = items as SequenceValue;
            if (arr != null)
            {
                return arr.Elements.Any(e => true.Equals(pred(Representation.Represent(e))));
            }

            var structure = items as StructureValue;
            if (structure != null)
            {
                return structure.Properties.Any(e => true.Equals(pred(Representation.Represent(e.Value))));
            }

            return Undefined.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_All(object items, object predicate)
        {
            var pred = predicate as Func<object, object>;
            if (pred == null)
                return Undefined.Value;

            SequenceValue arr = items as SequenceValue;
            if (arr != null)
            {
                return arr.Elements.All(e => true.Equals(pred(Representation.Represent(e))));
            }

            var structure = items as StructureValue;
            if (structure != null)
            {
                return structure.Properties.All(e => true.Equals(pred(Representation.Represent(e.Value))));
            }

            return Undefined.Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object TypeOf(object value)
        {
            var dict = value as StructureValue;
            if (dict == null)
                return Undefined.Value;

            return dict.TypeTag;
        }

        [AcceptUndefined, AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_IsNull(object value)
        {
            return value is Undefined || value == null;
        }

        [AcceptUndefined, AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object _Internal_IsNotNull(object value)
        {
            return value != null && !(value is Undefined);
        }

        [AcceptUndefined, AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Coalesce(object v1, object v2)
        {
            if (v1 != null && !(v1 is Undefined))
                return v1;

            return v2;
        }

        [AcceptNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Substring(object sval, object startIndex, object length)
        {
            var str = sval as string;
            if (str == null || !(startIndex is decimal) || length != null && !(length is decimal))
                return Undefined.Value;

            var si = (decimal)startIndex;

            if (si < 0 || si >= str.Length || (int)si != si)
                return Undefined.Value;

            if (length == null)
                return str.Substring((int)si);

            var len = (decimal)length;
            if ((int)len != len)
                return Undefined.Value;

            if (len + si > str.Length)
                return str.Substring((int)si);

            return str.Substring((int)si, (int)len);
        }
    }
}
