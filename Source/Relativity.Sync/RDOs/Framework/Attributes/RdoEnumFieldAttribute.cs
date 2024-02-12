namespace Relativity.Sync.RDOs.Framework.Attributes
{
    internal class RdoEnumFieldAttribute : RdoFieldAttribute
    {
        public RdoEnumFieldAttribute(string fieldGuid) : base(fieldGuid, RdoFieldType.FixedLengthText, fixedTextLength: 255, required: false)
        {
        }
    }
}
