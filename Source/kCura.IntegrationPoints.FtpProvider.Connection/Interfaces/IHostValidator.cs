namespace kCura.IntegrationPoints.FtpProvider.Connection.Interfaces
{
    public interface IHostValidator
    {
        bool CanConnectTo(string host);
    }
}
