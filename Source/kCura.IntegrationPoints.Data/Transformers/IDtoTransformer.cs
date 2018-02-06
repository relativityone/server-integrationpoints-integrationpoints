﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Transformers
{
    /// <summary>
    /// Transforms objects to their DTO representation
    /// </summary>
    public interface IDtoTransformer<T1, T2> 
        where T1 : BaseDTO
        where T2 : new()
    {
        /// <summary>
        /// Converts the data object to DTO representation
        /// </summary>
        /// <param name="transformee">Object being transformed</param>
        /// <returns></returns>
        T1 ConvertToDto(T2 transformee);

        /// <summary>
        /// Converts the data object to DTO representation
        /// </summary>
        /// <param name="transformees">Objects being transformed</param>
        /// <returns></returns>
        List<T1> ConvertToDto(IEnumerable<T2> transformees);

		/// <summary>
		/// Converts the generic Artifact DTO to DTO representation
		/// </summary>
		/// <param name="transformees">Objects being transformed</param>
		/// <returns>List of new DTOs</returns>
		T1 ConvertArtifactDtoToDto(ArtifactDTO transformees);
		
		/// <summary>
		/// Converts the generic Artifact DTOs to DTO representation
		/// </summary>
		/// <param name="transformees">Objects being transformed</param>
		/// <returns></returns>
		List<T1> ConvertArtifactDtoToDto(IEnumerable<ArtifactDTO> transformees);
	}
}