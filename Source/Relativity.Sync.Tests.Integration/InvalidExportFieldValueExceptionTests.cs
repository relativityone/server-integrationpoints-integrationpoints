using System;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Integration
{
    internal sealed class InvalidExportFieldValueExceptionTests : ExceptionSerializationTestsBase<InvalidExportFieldValueException>
    {
        protected override InvalidExportFieldValueException CreateException(string message, Exception innerException)
        {
            return new InvalidExportFieldValueException(message, innerException);
        }
    }
}
