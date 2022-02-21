using System.Collections.Generic;
using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace Relativity.IntegrationPoints.FieldsMapping
{
    public class FieldsClassifyRunnerFactory : IFieldsClassifyRunnerFactory
    {
        private const int DocumentArtifactTypeID = (int)ArtifactType.Document;

        private readonly IImportApiFacade _importApiFacade;
        private readonly IFieldsRepository _fieldsRepository;

        private readonly IList<IFieldsClassifier> _defaultDocumentSourceFieldsClassifiers;
        private readonly IList<IFieldsClassifier> _defaultDocumentDestinationFieldsClassifiers;

        private readonly IList<IFieldsClassifier> _defaultNonDocumentSourceFieldsClassifiers;
        private readonly IList<IFieldsClassifier> _defaultNonDocumentDestinationFieldsClassifiers;

        public FieldsClassifyRunnerFactory(IImportApiFacade importApiFacade, IFieldsRepository fieldsRepository)
        {
            _importApiFacade = importApiFacade;
            _fieldsRepository = fieldsRepository;

            _defaultDocumentSourceFieldsClassifiers = new List<IFieldsClassifier>
            {
                new RipFieldsClassifier(),
                new SystemFieldsClassifier(),
                new NotSupportedByIAPIFieldsClassifier(_importApiFacade, DocumentArtifactTypeID),
                new ObjectFieldsClassifier()
            };

            _defaultDocumentDestinationFieldsClassifiers = new List<IFieldsClassifier>
            {
                new RipFieldsClassifier(),
                new SystemFieldsClassifier(),
                new NotSupportedByIAPIFieldsClassifier(_importApiFacade, DocumentArtifactTypeID),
                new OpenToAssociationsFieldsClassifier(),
                new ObjectFieldsClassifier()
            };

            _defaultNonDocumentSourceFieldsClassifiers = new List<IFieldsClassifier>
            {
                new SystemFieldsClassifier(),
                new ObjectFieldsClassifier()
            };

            _defaultNonDocumentDestinationFieldsClassifiers = new List<IFieldsClassifier>
            {
                new SystemFieldsClassifier(),
                new OpenToAssociationsFieldsClassifier(),
                new ObjectFieldsClassifier()
            };
        }

        public IFieldsClassifierRunner CreateForSourceWorkspace(int artifactTypeId)
        {
            IList<IFieldsClassifier> classifiers;

            if (artifactTypeId == DocumentArtifactTypeID)
            {
                classifiers = _defaultDocumentSourceFieldsClassifiers;
            }
            else
            {
                classifiers = _defaultNonDocumentSourceFieldsClassifiers;
                classifiers.Add(new NotSupportedByIAPIFieldsClassifier(_importApiFacade, artifactTypeId));
            }

            return new FieldsClassifierRunner(_fieldsRepository, classifiers);
        }

        public IFieldsClassifierRunner CreateForDestinationWorkspace(int artifactTypeId)
        {
            IList<IFieldsClassifier> classifiers;

            if (artifactTypeId == DocumentArtifactTypeID)
            {
                classifiers = _defaultDocumentDestinationFieldsClassifiers;
            }
            else
            {
                classifiers = _defaultNonDocumentDestinationFieldsClassifiers;
                classifiers.Add(new NotSupportedByIAPIFieldsClassifier(_importApiFacade, artifactTypeId));
            }

            return new FieldsClassifierRunner(_fieldsRepository, classifiers);
        }
    }
}
