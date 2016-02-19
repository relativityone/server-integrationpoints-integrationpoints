using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.Marshaller
{
	internal interface IAppDomainDataMarshaller
	{
		/// <summary>
		/// Marshals data to the target AppDomain
		/// </summary>
		/// <param name="targetAppDomain">The AppDomain to send marshal the data to</param>
		/// <param name="dataDictionary">The data to set on the target AppDomain</param>
		void MarshalDataToDomain(AppDomain targetAppDomain, IDictionary<string, byte[]> dataDictionary);

		/// <summary>
		/// Retrieves the data for the corresponding dataKey on the source AppDomain
		/// </summary>
		/// <param name="sourceAppDomain">The AppDomain to look in for the data</param>
		/// <param name="dataKey">The key to use for finding the data</param>
		/// <returns>If no data exists NULL, otherwise, the data associated with the key</returns>
		byte[] RetrieveMarshaledData(AppDomain sourceAppDomain, string dataKey);
	}
}