using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Services.Marshaller
{
	internal interface IAppDomainDataMarshaller
	{
		void MarshalDataToDomain(AppDomain targetAppDomain, IDictionary<string, byte[]> dataDictionary);
		byte[] RetrieveMarshaledData(AppDomain sourceAppDomain, string dataKey);
	}
}