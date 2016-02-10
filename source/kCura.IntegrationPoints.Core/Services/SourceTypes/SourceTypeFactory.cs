using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
	public class SourceType
	{
		public string Name { get; set; }
		public string ID { get; set; }
		public string SourceURL { get; set; }
		public int ArtifactID { get; set; }
	}

	public class SourceTypeFactory : ISourceTypeFactory
	{
		private readonly ICaseServiceContext _context;
		public SourceTypeFactory(ICaseServiceContext context)
		{
			_context = context;
		}

		public virtual IEnumerable<SourceType> GetSourceTypes()
		{
			var types = _context.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(Data.SourceProviderFieldGuids.Name), Guid.Parse(Data.SourceProviderFieldGuids.Identifier), Guid.Parse(Data.SourceProviderFieldGuids.SourceConfigurationUrl));
			return types.Select(x => new SourceType { Name = x.Name, ID = x.Identifier, SourceURL = x.SourceConfigurationUrl, ArtifactID = x.ArtifactId}).OrderBy(x => x.Name).ToList();
		}
	}
}
