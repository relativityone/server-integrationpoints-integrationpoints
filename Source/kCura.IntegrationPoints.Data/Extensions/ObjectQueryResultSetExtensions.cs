using System;
using System.Linq;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.ObjectQuery;

namespace kCura.IntegrationPoints.Data.Extensions
{
	public static class ObjectQueryResultSetExtensions
	{
		/// <summary>
		/// Converts Object Query's DataResult into an array of ArtifactDTO
		/// </summary>
		/// <param name="resultSet"></param>
		/// <returns>an array of ArtifactDTO</returns>
		/// <exception cref="Exception">throws exception when given result set is failed to retrieve data</exception>
		public static ArtifactDTO[] GetResultsAsArtifactDto(this ObjectQueryResultSet resultSet)
		{
			if (resultSet == null)
			{
				throw new ArgumentNullException(nameof(resultSet));
			}

			if (resultSet.Success)
			{
				ArtifactDTO[] results = resultSet.Data.DataResults.Select(
					x => new ArtifactDTO(
						x.ArtifactId,
						x.ArtifactTypeId,
						x.TextIdentifier,
						x.Fields.Select(
							y => new ArtifactFieldDTO() { ArtifactId = y.ArtifactId, FieldType = y.FieldType, Name = y.Name, Value = y.Value }))
					).ToArray();

				return results;
			}

			throw new Exception(resultSet.Message);
		}
	}
}