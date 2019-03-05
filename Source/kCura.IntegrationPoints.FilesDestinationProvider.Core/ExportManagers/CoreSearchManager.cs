﻿using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.WinEDDS;
using kCura.WinEDDS.Service.Export;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Services.Interfaces.ViewField.Models;
using RelativityViewFieldInfo = Relativity.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreSearchManager : ISearchManager
	{
		private readonly BaseServiceContext _baseServiceContext;
		private readonly IViewFieldRepository _viewFieldRepository;

		public CoreSearchManager(BaseServiceContext baseServiceContext, IViewFieldRepository viewFieldRepository)
		{
			_baseServiceContext = baseServiceContext;
			_viewFieldRepository = viewFieldRepository;
		}

		public DataSet RetrieveNativesForSearch(int caseContextArtifactID, string documentArtifactIDs)
		{
			Init(caseContextArtifactID);
			return FileQuery.RetrieveNativesForDocuments(_baseServiceContext, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveNativesForProduction(int caseContextArtifactID, int productionArtifactID, string documentArtifactIDs)
		{
			Init(caseContextArtifactID);
			return FileQuery.RetrieveNativesForProductionDocuments(_baseServiceContext, productionArtifactID, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveFilesForDynamicObjects(int caseContextArtifactID, int fileFieldArtifactID, int[] objectIds)
		{
			Init(caseContextArtifactID);
			return FileQuery.RetrieveFilesForDynamicObjects(_baseServiceContext, fileFieldArtifactID, objectIds)?.ToDataSet();
		}

		public DataSet RetrieveImagesForProductionDocuments(int caseContextArtifactID, int[] documentArtifactIDs, int productionArtifactID)
		{
			return FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(_baseServiceContext, productionArtifactID, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveImagesForDocuments(int caseContextArtifactID, int[] documentArtifactIDs)
		{
			return FileQuery.RetrieveAllImagesForDocuments(_baseServiceContext, documentArtifactIDs).ToDataSet();
		}

		public DataSet RetrieveProducedImagesForDocument(int caseContextArtifactID, int documentArtifactID)
		{
			return FileQuery.RetrieveAllByDocumentArtifactIdAndType(_baseServiceContext, documentArtifactID, 3).ToDataSet(); // TODO 3 -> Type enum
		}

		public DataSet RetrieveImagesByProductionIDsAndDocumentIDsForExport(int caseContextArtifactID, int[] productionArtifactIDs, int[] documentArtifactIDs)
		{
			return FileQuery.RetrieveByProductionIDsAndDocumentIDsForExport(_baseServiceContext, productionArtifactIDs, documentArtifactIDs).ToDataSet();
		}

		public ViewFieldInfo[] RetrieveAllExportableViewFields(int caseContextArtifactID, int artifactTypeID)
		{
			ViewFieldResponse[] viewFieldResponseArray = _viewFieldRepository.ReadExportableViewFields(caseContextArtifactID, artifactTypeID);
			ViewFieldInfo[] viewFieldInfoArray = ToViewFieldInfoArray(viewFieldResponseArray);
			return viewFieldInfoArray;
		}

		private static ViewFieldInfo[] ToViewFieldInfoArray(ViewFieldResponse[] viewFieldResponseArray)
		{
			var viewFieldInfoList = new List<ViewFieldInfo>(viewFieldResponseArray.Length);

			foreach (var viewFieldResponse in viewFieldResponseArray)
			{
				RelativityViewFieldInfo coreViewFieldInfo = new CoreViewFieldInfo(viewFieldResponse);
				ViewFieldInfo viewFieldInfo = new ViewFieldInfo(coreViewFieldInfo);
				viewFieldInfoList.Add(viewFieldInfo);
			}

			return viewFieldInfoList.ToArray();
		}

		public int[] RetrieveDefaultViewFieldIds(int caseContextArtifactID, int viewArtifactID, int artifactTypeID, bool isProduction)
		{
			ViewFieldIDResponse[] viewFieldIDResponseArray = isProduction
				? _viewFieldRepository.ReadViewFieldIDsFromProduction(caseContextArtifactID, artifactTypeID, viewArtifactID)
				: _viewFieldRepository.ReadViewFieldIDsFromSearch(caseContextArtifactID, artifactTypeID, viewArtifactID);

			var result = new List<int>();
			foreach (var viewFieldIdResponse in viewFieldIDResponseArray)
			{
				if (viewFieldIdResponse.ArtifactID.Equals(viewArtifactID))
				{
					result.Add(viewFieldIdResponse.ArtifactViewFieldID);
				}
			}

			return result.ToArray();
		}

		public DataSet RetrieveViewsByContextArtifactID(int caseContextArtifactID, int artifactTypeID, bool isSearch)
		{
			return InitViewManager(caseContextArtifactID).ExternalRetrieveViews(_baseServiceContext, artifactTypeID, isSearch);
		}

		public void Dispose()
		{
		}

		private ViewManager InitViewManager(int appArtifactId)
		{
			Init(appArtifactId);
			return new ViewManager();
		}

		private void Init(int appArtifactId)
		{
			_baseServiceContext.AppArtifactID = appArtifactId;
		}
	}
}