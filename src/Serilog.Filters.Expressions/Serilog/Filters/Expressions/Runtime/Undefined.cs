namespace Serilog.Filters.Expressions.Runtime
{
    sealed class Undefined
    {
        public static readonly Undefined Value = new Undefined();

        private Undefined() { }

        public override int GetHashCode()
        {
            return 0;
        }

        public override bool Equals(object obj)
        {
            return false;
        }
    }
}
