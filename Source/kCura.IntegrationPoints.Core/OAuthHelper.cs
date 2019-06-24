using System;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core
{
	public class OAuthHelper : IHelper
	{
		private readonly Uri _instanceUri;
		private readonly Uri _rsapiUri;
		private readonly Uri _keplerUri;
		private readonly OAuthClientDto _oAuthClientDto;
		private readonly ITokenProvider _tokenProvider;
		private readonly IHelper _sourceHelper;
		

		public OAuthHelper(
			IHelper sourceHelper, 
			Uri instanceUri, 
			Uri rsapiUri, 
			Uri keplerUri, 
			OAuthClientDto oAuthClientDto, 
			ITokenProvider tokenProvider)
		{
			_sourceHelper = sourceHelper;
			_instanceUri = instanceUri;
			_rsapiUri = rsapiUri;
			_keplerUri = keplerUri;
			_oAuthClientDto = oAuthClientDto;
			_tokenProvider = tokenProvider;
		}

		public void Dispose()
		{
			//nothing to dispose
		}

		public IDBContext GetDBContext(int caseID)
		{
			throw new NotImplementedException();
		}

		public IServicesMgr GetServicesManager()
		{
			return new FederatedInstanceServicesManager(
				_keplerUri,
				_rsapiUri,
				_instanceUri,
				_oAuthClientDto,
				_tokenProvider
			);
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new NotImplementedException();
		}

		public ILogFactory GetLoggerFactory()
		{
			return _sourceHelper.GetLoggerFactory();
		}

		public string ResourceDBPrepend()
		{
			throw new NotImplementedException();
		}

		public string ResourceDBPrepend(IDBContext context)
		{
			throw new NotImplementedException();
		}

		public string GetSchemalessResourceDataBasePrepend(IDBContext context)
		{
			throw new NotImplementedException();
		}

		public Guid GetGuid(int workspaceID, int artifactID)
		{
			throw new NotImplementedException();
		}

		public ISecretStore GetSecretStore()
		{
			throw new NotImplementedException();
		}

		public IInstanceSettingsBundle GetInstanceSettingBundle()
		{
			throw new NotImplementedException();
		}

		public IStringSanitizer GetStringSanitizer(int workspaceID)
		{
			throw new NotImplementedException();
		}
	}
}