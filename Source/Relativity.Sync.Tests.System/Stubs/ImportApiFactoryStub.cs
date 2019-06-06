﻿using System;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;
using Relativity.Sync.Executors;

namespace Relativity.Sync.Tests.System.Stubs
{
	internal sealed class ImportApiFactoryStub : IImportApiFactory
	{
		private readonly string _userName;
		private readonly string _password;

		public ImportApiFactoryStub(string userName, string password)
		{
			_userName = userName;
			_password = password;
		}

		public async Task<IImportAPI> CreateImportApiAsync(Uri webServiceUrl)
		{
			return await Task.Run(() => new ImportAPI(_userName, _password, webServiceUrl.AbsoluteUri)).ConfigureAwait(false);
		}
	}
}