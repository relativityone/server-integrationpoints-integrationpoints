﻿using System.Diagnostics;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Scripts
{
	public class CommonScriptsFactory : ICommonScriptsFactory
	{
		private readonly string _apiControllerName;
		private readonly ICaseServiceContext _caseServiceContext;
		private readonly IIntegrationPointBaseFieldsConstants _fieldsConstants;
		private readonly IIntegrationPointBaseFieldGuidsConstants _guidsConstants;

		public CommonScriptsFactory(IEHHelper helper, IIntegrationPointBaseFieldGuidsConstants guidsConstants, IIntegrationPointBaseFieldsConstants fieldsConstants,
			string apiControllerName)
		{
			_guidsConstants = guidsConstants;
			_fieldsConstants = fieldsConstants;
			_apiControllerName = apiControllerName;
			_caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(helper, helper.GetActiveCaseID());
		}

		public ICommonScripts Create(EventHandlerBase eventHandlerBase)
		{
			int sourceProviderId = (int) eventHandlerBase.ActiveArtifact.Fields[_fieldsConstants.SourceProvider].Value.Value;
			var sourceProviderArtifact = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(sourceProviderId);

			int destinationProviderId = (int) eventHandlerBase.ActiveArtifact.Fields[_fieldsConstants.DestinationProvider].Value.Value;
			var destinationProviderArtifact = _caseServiceContext.RsapiService.DestinationProviderLibrary.Read(destinationProviderId);

			if (sourceProviderArtifact.Name == Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME)
			{
				if (destinationProviderArtifact.Name == Constants.IntegrationPoints.FILESHARE_PROVIDER_NAME)
				{
					return CreateForLoadFile(eventHandlerBase);
				}
				return CreateForRelativity(eventHandlerBase);
			}
			return CreateForDefault(eventHandlerBase);
		}

		private ICommonScripts CreateForLoadFile(EventHandlerBase eventHandler)
		{
			return new LoadFileProviderScripts(new ScriptsHelper(eventHandler, _caseServiceContext, _fieldsConstants, _apiControllerName), _guidsConstants,
				new WorkspaceNameValidator(eventHandler.Helper),
				new FolderPathInformation(eventHandler.Helper.GetDBContext(eventHandler.Helper.GetActiveCaseID())));
		}

		private ICommonScripts CreateForRelativity(EventHandlerBase eventHandler)
		{
			return new RelativityProviderScripts(new ScriptsHelper(eventHandler, _caseServiceContext, _fieldsConstants, _apiControllerName), _guidsConstants,
				new WorkspaceNameValidator(eventHandler.Helper),
				new FolderPathInformation(eventHandler.Helper.GetDBContext(eventHandler.Helper.GetActiveCaseID())));
		}

		private ICommonScripts CreateForDefault(EventHandlerBase eventHandler)
		{
			return new ImportProvidersScripts(new ScriptsHelper(eventHandler, _caseServiceContext, _fieldsConstants, _apiControllerName), _guidsConstants);
		}
	}
}