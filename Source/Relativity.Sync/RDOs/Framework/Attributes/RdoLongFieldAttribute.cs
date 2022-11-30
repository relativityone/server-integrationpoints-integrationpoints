namespace Relativity.Sync.RDOs.Framework.Attributes
{
    internal class RdoLongFieldAttribute : RdoFieldAttribute
    {
        private const int LongMaxValueDigitCount = 19; // long.MaxValue.ToString().Length

        public RdoLongFieldAttribute(string fieldGuid, bool isRequired = false) : base(fieldGuid, RdoFieldType.FixedLengthText, LongMaxValueDigitCount, isRequired) { }
    }
}
