using System;
using System.Collections.Generic;
using Relativity.IntegrationPoints.Contracts.Models;

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