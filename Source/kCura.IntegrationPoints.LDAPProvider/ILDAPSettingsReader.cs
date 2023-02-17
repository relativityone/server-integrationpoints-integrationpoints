namespace kCura.IntegrationPoints.LDAPProvider
{
    public interface ILDAPSettingsReader
    {
        LDAPSettings GetSettings(string sourceConfiguration);
    }
}
