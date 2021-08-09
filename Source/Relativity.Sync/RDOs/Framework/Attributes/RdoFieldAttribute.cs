using System;

namespace Relativity.Sync.RDOs.Framework.Attributes
{
    internal class RdoFieldAttribute : Attribute
    {
        public RdoFieldType FieldType { get; }
        public int FixedTextLength { get; }
        public bool Required { get; }
        public Guid FieldGuid { get; }
        
        /// <summary>
        /// Attribute defining RDO field
        /// </summary>
        /// <param name="fieldGuid">GUID for the field</param>
        /// <param name="fieldType">Type of the field in database</param>
        /// <param name="fixedTextLength">Length of fixed-length text field</param>
        /// <param name="required">Whether the field is optional. Translates to nullable column in database</param>
        public RdoFieldAttribute(string fieldGuid, RdoFieldType fieldType, int fixedTextLength = 255, bool required = false)
        {
            FieldType = fieldType;
            FixedTextLength = fixedTextLength;
            Required = required;
            FieldGuid = Guid.Parse(fieldGuid);
        }

    }
}