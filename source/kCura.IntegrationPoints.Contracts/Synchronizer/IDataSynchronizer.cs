﻿using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Contracts.Synchronizer
{
	/// <summary>
	/// Imports data from the source provider into Relativity.
	/// </summary>
	public interface IDataSynchronizer : IFieldProvider
	{
		/// <summary>
		/// Synchronizes data from the data source provider and imports it into Relativity.
		/// </summary>
		/// <param name="data">The records to insert into the system.</param>
		/// <param name="fieldMap">The field mapping used to import data into the system.</param>
		/// <param name="options">The option settings used to synchronize the source data with the destination.</param>
		void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options);

		/// <summary>
		/// Synchronizes data from the data source provider and imports it into Relativity.
		/// </summary>
		/// <param name="data">The reader used to read the records to insert into the system.</param>
		/// <param name="fieldMap">The field mapping used to import data into the system.</param>
		/// <param name="options">The option settings used to synchronize the source data with the destination.</param>
		void SyncData(IDataReader data, IEnumerable<FieldMap> fieldMap, string options);
	}
}
