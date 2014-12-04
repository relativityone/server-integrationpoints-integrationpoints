using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Core.Services.SourceTypes
{
	public class SourceType
	{
		public string Name { get; set; }
		public string ID { get; set; }
	}

	public class SourceTypeFactory
	{
		private readonly IServiceContext _context;
		public SourceTypeFactory(IServiceContext context)
		{
			_context = context;
		}

		public virtual IEnumerable<SourceType> GetSourceTypes()
		{
			var types = _context.RsapiService.SourceProviderLibrary.ReadAll(Guid.Parse(Data.SourceProviderFieldGuids.Name), Guid.Parse(Data.SourceProviderFieldGuids.Identifier));
			return types.Select(x => new SourceType { Name = x.Name, ID = x.Identifier });
		}
	}
}
