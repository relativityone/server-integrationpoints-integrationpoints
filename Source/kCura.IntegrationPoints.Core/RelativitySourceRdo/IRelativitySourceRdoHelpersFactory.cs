using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public interface IRelativitySourceRdoHelpersFactory
    {
        IRelativitySourceRdoDocumentField CreateRelativitySourceRdoDocumentField(IRelativityProviderObjectRepository relativityProviderObjectRepository);
        IRelativitySourceRdoFields CreateRelativitySourceRdoFields();
        IRelativitySourceRdoObjectType CreateRelativitySourceRdoObjectType(IRelativityProviderObjectRepository relativityProviderObjectRepository);
    }
}