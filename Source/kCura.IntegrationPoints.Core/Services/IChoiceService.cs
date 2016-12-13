using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Core.Services
{
	public interface IChoiceService
	{
		List<FieldEntry> ConvertToFieldEntries(List<Relativity.Client.Artifact> artifacts);
		List<FieldEntry> GetChoiceFields(int rdoTypeId);
		List<Relativity.Client.DTOs.Choice> GetChoicesOnField(int fieldArtifactID);
		List<Relativity.Client.DTOs.Choice> GetChoicesOnField(Guid fieldGuid);
	}
}