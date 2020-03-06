using Relativity.IntegrationPoints.FieldsMapping.FieldClassifiers;
using kCura.Relativity.ImportAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.API;

namespace Relativity.IntegrationPoints.FieldsMapping
{
	public class FieldsClassifyRunnerFactory : IFieldsClassifyRunnerFactory
	{
		private readonly IImportAPI _importApi;
		private readonly IFieldsRepository _fieldsRepository;

		private readonly IList<IFieldsClassifier> _defaultSourceFieldsClassifiers;
		private readonly IList<IFieldsClassifier> _defaultDestinationFieldsClassifiers;

		public FieldsClassifyRunnerFactory(IImportAPI importAPI, IFieldsRepository fieldsRepository)
		{
			_importApi = importAPI;
			_fieldsRepository = fieldsRepository;

			_defaultSourceFieldsClassifiers = new List<IFieldsClassifier>
			{
				new RipFieldsClassifier(),
				new SystemFieldsClassifier(),
				new NotSupportedByIAPIFieldsClassifier(_importApi),
				new ObjectFieldsClassifier()
			};

			_defaultDestinationFieldsClassifiers = new List<IFieldsClassifier>
			{
				new RipFieldsClassifier(),
				new SystemFieldsClassifier(),
				new NotSupportedByIAPIFieldsClassifier(_importApi),
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
