namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public interface IFactoryConfigBuilder
    {
        ExporterFactoryConfig BuildFactoryConfig(ExportDataContext exportDataContext);
    }
}