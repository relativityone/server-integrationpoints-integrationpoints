namespace Relativity.Sync.Tests.System
{
    internal interface IDataReaderRowSetValidator
    {
        void ValidateAndRegisterRead(string controlNumber, params FieldVerifyData[] fieldVerifyData);

        void ValidateAllRead();
    }
}
