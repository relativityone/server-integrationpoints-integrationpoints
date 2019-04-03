using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
	public class SourceType
	{
		public string Name { get; set; }
		public string ID { get; set; }
		public string SourceURL { get; set; }
		public int ArtifactID { get; set; }
		public SourceProviderConfiguration Config { get; set; }
	}

	public class SourceTypeFactory : ISourceTypeFactory
	{
		private readonly IRelativityObjectManager _objectManager;

		public SourceTypeFactory(IRelativityObjectManager objectManager)
		{
			_objectManager = objectManager;
		}

		public virtual IEnumerable<SourceType> GetSourceTypes()
		{
			var request = new QueryRequest
			{
				Fields = new Data.SourceProvider().ToFieldList()
			};

			IList<Data.SourceProvider> types = _objectManager.Query<Data.SourceProvider>(request);
			return types.Select(x => new SourceType { Name = x.Name, ID = x.Identifier, SourceURL = x.SourceConfigurationUrl, ArtifactID = x.ArtifactId, Config = x.Config }).OrderBy(x => x.Name).ToList();
		}
	}
}