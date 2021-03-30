using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers
{
	public class FieldsMappingHelper : HelperBase
	{
		public FieldsMappingHelper(HelperManager helperManager, InMemoryDatabase database, ProxyMock proxyMock) 
			: base(helperManager, database, proxyMock)
		{
		}

		public List<FieldMap> PrepareIdentifierFieldsMapping(WorkspaceTest sourceWorkspace, WorkspaceTest destinationWorkspace)
		{
			FieldTest sourceControlNumber = Database.Fields.First(x =>
				x.WorkspaceId == sourceWorkspace.ArtifactId && x.IsIdentifier);
			FieldTest destinationControlNumber = Database.Fields.First(x =>
				x.WorkspaceId == destinationWorkspace.ArtifactId && x.IsIdentifier);

			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = sourceControlNumber.Name,
						FieldIdentifier = sourceControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					DestinationField = new FieldEntry
					{
						DisplayName = destinationControlNumber.Name,
						FieldIdentifier = destinationControlNumber.ArtifactId.ToString(),
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					FieldMapType = FieldMapTypeEnum.Identifier
				}
			};
		}
	}
}
