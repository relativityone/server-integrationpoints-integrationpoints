﻿using kCura.Relativity.ImportAPI;
using Relativity.DataExchange;

namespace Relativity.Sync.Executors
{
	public class ExtendedImportAPI : IExtendedImportAPI
	{
		public ImportAPI CreateByTokenProvider(string webServiceUrl, IRelativityTokenProvider relativityTokenProvider)
		{
			return kCura.Relativity.ImportAPI.ExtendedImportAPI.CreateByTokenProvider(webServiceUrl, relativityTokenProvider);
		
		}

	}
}