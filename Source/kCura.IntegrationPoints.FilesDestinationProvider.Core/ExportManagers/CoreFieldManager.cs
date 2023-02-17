using kCura.EDDS.WebAPI.FieldManagerBase;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.WinEDDS.Service.Export;
using Relativity.API.Foundation;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
    public class CoreFieldManager : IFieldManager
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly FieldConverter _converter;

        public CoreFieldManager(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _converter = new FieldConverter();
        }

        public Field Read(int caseContextArtifactID, int fieldArtifactID)
        {
            IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(caseContextArtifactID);
            IField field = fieldRepository.Read(fieldArtifactID);

            return _converter.ConvertToField(field);
        }
    }
}
