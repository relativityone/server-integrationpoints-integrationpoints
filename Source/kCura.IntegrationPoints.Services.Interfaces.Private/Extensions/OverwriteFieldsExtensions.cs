using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoints.Services.Interfaces.Private.Extensions
{
	public static class OverwriteFieldsExtensions
	{
		public static OverwriteFieldsModel ToModel(this Choice choice)
		{
			return new OverwriteFieldsModel
			{
				Name = choice.Name,
				ArtifactId = choice.ArtifactID
			};
		}
	}
}