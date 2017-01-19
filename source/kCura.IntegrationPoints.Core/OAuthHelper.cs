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

		public OAuthHelper(Uri instanceUri, Uri rsapiUri, Uri keplerUri, OAuthClientDto oAuthClientDto, ITokenProvider tokenProvider)
		{
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
			IServicesMgr servicesMgr = new OAuthServicesManager(_instanceUri, _rsapiUri, _keplerUri, _oAuthClientDto, _tokenProvider);

			return servicesMgr;
		}

		public IUrlHelper GetUrlHelper()
		{
			throw new System.NotImplementedException();
		}

		public ILogFactory GetLoggerFactory()
		{
			throw new System.NotImplementedException();
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
	}
}