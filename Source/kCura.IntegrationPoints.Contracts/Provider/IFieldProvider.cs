﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts.Models;

namespace kCura.IntegrationPoints.Contracts.Provider
{
	/// <summary>
	/// Retrieves fields from a data source.
	/// </summary>
    public interface IFieldProvider
	{
		/// <summary>
		/// Retrieves the type of a field.
		/// </summary>
		/// <param name="providerConfiguration">A group of configuration settings that the control source provider behavior.</param>
		/// <returns>Returns fields from a data source.</returns>
		IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration);
	}
}
