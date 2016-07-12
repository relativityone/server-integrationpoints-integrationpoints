﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Core.Services.DestinationTypes
{
	public class DestinationType
	{
		public string Name { get; set; }
		public string ID { get; set; }
		public int ArtifactID { get; set; }
	}

	public interface IDestinationTypeFactory
	{
		IEnumerable<DestinationType> GetDestinationTypes();
	}

	public class DestinationTypeFactory : IDestinationTypeFactory
	{
		private readonly ICaseServiceContext _context;

		public DestinationTypeFactory(ICaseServiceContext context)
		{
			_context = context;
		}

		public virtual IEnumerable<DestinationType> GetDestinationTypes()
		{
			var types = _context.RsapiService.DestinationProviderLibrary.ReadAll(Guid.Parse(DestinationProviderFieldGuids.Name), Guid.Parse(Data.DestinationProviderFieldGuids.Identifier));
			return types.Select(x => new DestinationType { Name = x.Name, ID = x.Identifier, ArtifactID = x.ArtifactId }).OrderBy(x => x.Name).ToList();
		}
	}
}