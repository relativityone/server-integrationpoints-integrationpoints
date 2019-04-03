using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Services.Provider
{
	public class GetSourceProviderRdoByApplicationIdentifier
	{
		private readonly IRelativityObjectManager _relativityObjectManager;

		public GetSourceProviderRdoByApplicationIdentifier(IRelativityObjectManager relativityObjectManager)
		{
			_relativityObjectManager = relativityObjectManager;
		}

		public List<SourceProvider> Execute(Guid appGuid)
		{
			var request = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = Guid.Parse(ObjectTypeGuids.SourceProvider)
				},
				Fields = RDOConverter.ConvertPropertiesToFields<SourceProvider>(),
				Condition = $"'{SourceProviderFields.ApplicationIdentifier}' == '{appGuid}'"
			};
			return _relativityObjectManager.Query<SourceProvider>(request);
		}
	}
}
