﻿using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.RelativitySync.Adapters
{
	internal class RdoRepository : IRdoRepository
	{
		private readonly IAPILog _logger;
		private readonly ICaseServiceContext _caseServiceContext;

		public RdoRepository(IWindsorContainer ripContainer)
		{
			_logger = ripContainer.Resolve<IAPILog>();
			_caseServiceContext = ripContainer.Resolve<ICaseServiceContext>();
		}

		public T Get<T>(int artifactId) where T : BaseRdo, new()
		{
			string typeName = nameof(T);
			_logger.LogDebug("Reading object of type {typeName} with artifact ID: {artifactId} using ObjectManager", typeName, artifactId);
			T readObject = _caseServiceContext.RsapiService.RelativityObjectManager.Read<T>(artifactId);
			_logger.LogDebug("Successfuly retrieved object of type {typeName} with artifact ID: {artifactId} using ObjectManager", typeName, artifactId);
			return readObject;
		}
	}
}