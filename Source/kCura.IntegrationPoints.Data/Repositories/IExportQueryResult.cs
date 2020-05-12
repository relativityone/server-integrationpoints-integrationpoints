﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Object provides a way to safely get results from export in block or all at once.
	///
	/// Export table is cleared on Dispose
	/// </summary>
	public interface IExportQueryResult : IDisposable
	{
		/// <summary>
		/// Retrieves block of results.
		/// </summary>
		/// <param name="startIndex">Starting index</param>
		/// <param name="resultsBlockSize">Number of retrieved results</param>
		/// <returns>Results</returns>
		Task<IEnumerable<RelativityObjectSlim>> GetNextBlockAsync(int startIndex, int resultsBlockSize = 1000);

		/// <summary>
		/// Retrieves all results, always starts at index 0
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<RelativityObjectSlim>> GetAllResultsAsync();

		/// <summary>
		/// Export Initialization result
		/// </summary>
		ExportInitializationResults ExportResult { get; }
	}
}