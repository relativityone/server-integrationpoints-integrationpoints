using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using System.Collections.Generic;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class FieldsClassifyRunnerFactory : IFieldsClassifyRunnerFactory
	{
		private readonly IFieldsMappingImportApiFacade _importApiFacade;
		private readonly IFieldsRepository _fieldsRepository;

		private readonly IList<IFieldsClassifier> _defaultSourceFieldsClassifiers;
		private readonly IList<IFieldsClassifier> _defaultDestinationFieldsClassifiers;

		public FieldsClassifyRunnerFactory(IFieldsMappingImportApiFacade importApiFacade, IFieldsRepository fieldsRepository)
		{
			_importApiFacade = importApiFacade;
			_fieldsRepository = fieldsRepository;

			_defaultSourceFieldsClassifiers = new List<IFieldsClassifier>
			{
				new RipFieldsClassifier(),
				new SystemFieldsClassifier(),
				new NotSupportedByIAPIFieldsClassifier(_importApiFacade),
				new ObjectFieldsClassifier()
			};

			_defaultDestinationFieldsClassifiers = new List<IFieldsClassifier>
			{
				new RipFieldsClassifier(),
				new SystemFieldsClassifier(),
				new NotSupportedByIAPIFieldsClassifier(_importApiFacade),
				new OpenToAssociationsFieldsClassifier(),
				new ObjectFieldsClassifier()
			};
		}

		public IFieldsClassifierRunner CreateForDestinationWorkspace()
		{
			return new FieldsClassifierRunner(_fieldsRepository, _defaultSourceFieldsClassifiers);
		}

		public IFieldsClassifierRunner CreateForSourceWorkspace()
		{
			return new FieldsClassifierRunner(_fieldsRepository, _defaultDestinationFieldsClassifiers);
		}
	}
}
