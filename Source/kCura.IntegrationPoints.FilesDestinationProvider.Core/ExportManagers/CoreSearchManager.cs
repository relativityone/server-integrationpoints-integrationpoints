using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Repositories;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Utilities;
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
		private readonly IFileRepository _fileRepository;
		private readonly IFileFieldRepository _fileFieldRepository;
		private readonly IViewFieldRepository _viewFieldRepository;

		public CoreSearchManager(
			BaseServiceContext baseServiceContext, 
			IFileRepository fileRepository, 
			IFileFieldRepository fileFieldRepository, 
			IViewFieldRepository viewFieldRepository)
		{
			_baseServiceContext = baseServiceContext;
			_fileRepository = fileRepository;
			_fileFieldRepository = fileFieldRepository;
			_viewFieldRepository = viewFieldRepository;
		}

		public DataSet RetrieveNativesForSearch(int caseContextArtifactID, string documentArtifactIDs)
		{
			ThrowIfDocumentArtifactIDsInvalid(documentArtifactIDs);

			int[] documentIDs = CommaSeparatedNumbersToArrayConverter
				.Convert(documentArtifactIDs);

			return _fileRepository
				.GetNativesForSearch(caseContextArtifactID, documentIDs)
				.ToDataSet();
		}

		public DataSet RetrieveNativesForProduction(
			int caseContextArtifactID, 
			int productionArtifactID,
			string documentArtifactIDs)
		{
			ThrowIfDocumentArtifactIDsInvalid(documentArtifactIDs);

			int[] documentIDs = CommaSeparatedNumbersToArrayConverter
				.Convert(documentArtifactIDs);
			return _fileRepository
				.GetNativesForProduction(caseContextArtifactID, productionArtifactID, documentIDs)
				.ToDataSet();
		}

		public DataSet RetrieveFilesForDynamicObjects(
			int caseContextArtifactID, 
			int fileFieldArtifactID,
			int[] objectIds)
		{
			return _fileFieldRepository
				.GetFilesForDynamicObjects(caseContextArtifactID, fileFieldArtifactID, objectIds)
				.ToDataSet();
		}

		public DataSet RetrieveImagesForProductionDocuments(
			int caseContextArtifactID, 
			int[] documentArtifactIDs,
			int productionArtifactID)
		{
			return _fileRepository
				.GetImagesForProductionDocuments(caseContextArtifactID, productionArtifactID, documentArtifactIDs)
				.ToDataSet();
		}

		public DataSet RetrieveImagesForDocuments(int caseContextArtifactID, int[] documentArtifactIDs)
		{
			return _fileRepository
				.GetImagesForDocuments(caseContextArtifactID, documentArtifactIDs)
				.ToDataSet();
		}

		public DataSet RetrieveProducedImagesForDocument(int caseContextArtifactID, int documentArtifactID)
		{
			return _fileRepository
				.GetProducedImagesForDocument(caseContextArtifactID, documentArtifactID)
				.ToDataSet();
		}

		public DataSet RetrieveImagesByProductionIDsAndDocumentIDsForExport(
			int caseContextArtifactID,
			int[] productionArtifactIDs, 
			int[] documentArtifactIDs)
		{
			return _fileRepository
				.GetImagesForExport(caseContextArtifactID, productionArtifactIDs, documentArtifactIDs)
				.ToDataSet();
		}

		public ViewFieldInfo[] RetrieveAllExportableViewFields(int caseContextArtifactID, int artifactTypeID)
		{
			return _viewFieldRepository
				.ReadExportableViewFields(caseContextArtifactID, artifactTypeID)
				.Select(ToViewFieldInfo)
				.ToArray();
		}

		public int[] RetrieveDefaultViewFieldIds(
			int caseContextArtifactID, 
			int viewArtifactID, 
			int artifactTypeID,
			bool isProduction)
		{
			ViewFieldIDResponse[] viewFieldIDResponseArray = isProduction
				? _viewFieldRepository.ReadViewFieldIDsFromProduction(
					caseContextArtifactID, 
					artifactTypeID,
					viewArtifactID
				  )
				: _viewFieldRepository.ReadViewFieldIDsFromSearch(
					caseContextArtifactID, 
					artifactTypeID,
					viewArtifactID
				  );

			return viewFieldIDResponseArray
				.Where(x => x.ArtifactID.Equals(viewArtifactID))
				.Select(x => x.ArtifactViewFieldID)
				.ToArray();
		}

		public DataSet RetrieveViewsByContextArtifactID(int caseContextArtifactID, int artifactTypeID, bool isSearch)
		{
			return InitViewManager(caseContextArtifactID)
				.ExternalRetrieveViews(_baseServiceContext, artifactTypeID, isSearch);
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

		private static ViewFieldInfo ToViewFieldInfo(ViewFieldResponse viewFieldResponse)
		{
			RelativityViewFieldInfo coreViewFieldInfo = new CoreViewFieldInfo(viewFieldResponse);
			return new ViewFieldInfo(coreViewFieldInfo);
		}

		private void ThrowIfDocumentArtifactIDsInvalid(string documentIDsAsString)
		{
			bool invalid = !IsArtifactIDsStringValid(documentIDsAsString);

			if (invalid)
			{
				throw new ArgumentException("Invalid documentArtifactIDs");
			}
		}

		//validation logic ported from FileQuery
		//remove it when we will be able to pass array instead of a string
		private bool IsArtifactIDsStringValid(string artifactIDsAsString)
		{
			if (artifactIDsAsString == null)
			{
				return false;
			}
			var regex = new Regex("^([0-9 ,]+)$");
			return regex.IsMatch(artifactIDsAsString);
		}

	}
}