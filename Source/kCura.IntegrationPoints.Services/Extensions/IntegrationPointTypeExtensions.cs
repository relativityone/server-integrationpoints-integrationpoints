﻿using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.Services.Extensions
{
	public static class IntegrationPointTypeExtensions
	{
		public static IntegrationPointTypeModel ToModel(this IntegrationPointType rdo)
		{
			return new IntegrationPointTypeModel
			{
				Name = rdo.Name,
				ArtifactId = rdo.ArtifactId
			};
		}
	}
}