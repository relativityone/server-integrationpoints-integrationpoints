using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Helpers.WorkspaceHelpers
{
	public class FieldsMappingHelper : WorkspaceHelperBase
	{	
		public FieldsMappingHelper(WorkspaceTest workspace) : base(workspace)
		{
		}
	
		public List<FieldMap> PrepareIdentifierFieldsMapping(WorkspaceTest destinationWorkspace)
		{
			FieldTest sourceControlNumber = Workspace.Fields.First(x => x.IsIdentifier);
			
			FieldTest destinationControlNumber = destinationWorkspace.Fields.First(x => x.IsIdentifier);

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
		
		public List<FieldMap> PrepareIdentifierFieldsMappingForImport(string identifierFieldName)
		{
			FieldTest sourceControlNumber = Workspace.Fields.First(x => x.IsIdentifier);
			
			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField =new FieldEntry
					{
						DisplayName = identifierFieldName,
						FieldIdentifier = "0",
						FieldType = FieldType.String,
						IsIdentifier = true,
						IsRequired = true,
						Type = ""
					},
					DestinationField = new FieldEntry
					{
						DisplayName = sourceControlNumber.Name,
						FieldIdentifier = sourceControlNumber.ArtifactId.ToString(),
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
