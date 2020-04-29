using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Data.UtilityDTO;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories
{
	/// <summary>
	/// Object internally tracks current position in exported set. Provides a way to safely get results in block or all at once.
	///
	/// Starting index can be reset by setting the <see cref="NextBlockStartIndex"/> property
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
		/// Export Guid
		/// </summary>
		Guid RunId { get; }
	}
}