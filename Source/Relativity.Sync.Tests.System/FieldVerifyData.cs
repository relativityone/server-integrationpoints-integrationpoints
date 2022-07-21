using System;

namespace Relativity.Sync.Tests.System
{
    internal class FieldVerifyData
    {
        public string ColumnName { get; set; }
        public object ActualValue { get; set; }
        public Action<string, object, object> Validator { get; set; }
    }
}