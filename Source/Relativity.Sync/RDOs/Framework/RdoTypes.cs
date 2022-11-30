using System;
using System.Collections.Generic;
using System.Reflection;

namespace Relativity.Sync.RDOs.Framework
{
    internal struct RdoTypeInfo
    {
        public Guid TypeGuid { get; set; }

        public string Name { get; set; }

        public Guid? ParentTypeGuid { get; set; }

        public IReadOnlyDictionary<Guid, RdoFieldInfo> Fields { get; set; }
    }

    internal struct RdoFieldInfo
    {
        public string Name { get; set; }

        public Guid Guid { get; set; }

        public RdoFieldType Type { get; set; }

        public bool IsRequired { get; set; }

        public int TextLength { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
    }

    internal enum RdoFieldType
    {
        WholeNumber,
        Decimal,
        FixedLengthText,
        LongText,
        YesNo
    }
}
