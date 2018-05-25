using System;
using System.Collections.Generic;
using System.Net;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.WinEDDS.Api;
using Relativity.API;
using Relativity.APIHelper;
using Relativity.APIHelper.ServiceManagers.ProxyHandlers;
using Relativity.Services.Pipeline;

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
		

		public OAuthHelper(IHelper sourceHelper, Uri instanceUri, Uri rsapiUri, Uri keplerUri, OAuthClientDto oAuthClientDto, ITokenProvider tokenProvider)
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
			throw new System.NotImplementedException();
		}

		public IDBContext GetDBContext(int caseID)
		{
			throw new System.NotImplementedException();
		}

		public IServicesMgr GetServicesManager()
		{
			IServicesMgr servicesMgr = new ServicesManagerBase(_rsapiUri, _keplerUri, WireProtocolVersion.V2, new IProxyHandler[]
			{
				new FederatedInstanceKeplerHandler(_instanceUri, _oAuthClientDto, _tokenProvider),
				new FederatedInstanceRsapiHandler(_instanceUri, _oAuthClientDto, _tokenProvider)
			});

			return servicesMgr;
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new System.NotImplementedException();
		}

		public ILogFactory GetLoggerFactory()
		{
			return _sourceHelper.GetLoggerFactory();
		}

		public string ResourceDBPrepend()
		{
			throw new System.NotImplementedException();
		}

		public string ResourceDBPrepend(IDBContext context)
		{
			throw new System.NotImplementedException();
		}

		public string GetSchemalessResourceDataBasePrepend(IDBContext context)
		{
			throw new System.NotImplementedException();
		}

		public Guid GetGuid(int workspaceID, int artifactID)
		{
			throw new System.NotImplementedException();
		}

		public ISecretStore GetSecretStore()
		{
			throw new NotImplementedException();
		}

		public IInstanceSettingsBundle GetInstanceSettingBundle()
		{
			throw new NotImplementedException();
		}
	}
}