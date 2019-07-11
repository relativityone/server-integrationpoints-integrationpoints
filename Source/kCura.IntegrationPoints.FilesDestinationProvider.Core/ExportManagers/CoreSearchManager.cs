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
using Relativity.Services.Interfaces.ViewField.Models;
using Relativity.Services.ViewManager.Models;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers
{
	public class CoreSearchManager : ISearchManager
	{
		private readonly IFileFieldRepository _fileFieldRepository;
		private readonly IFileRepository _fileRepository;
		private readonly IViewFieldRepository _viewFieldRepository;
		private readonly IViewRepository _viewRepository;

		public CoreSearchManager(
			IFileRepository fileRepository, 
			IFileFieldRepository fileFieldRepository, 
			IViewFieldRepository viewFieldRepository,
			IViewRepository viewRepository)
		{
			_fileRepository = fileRepository;
			_fileFieldRepository = fileFieldRepository;
			_viewFieldRepository = viewFieldRepository;
			_viewRepository = viewRepository;
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
			DataSet dataSet;

			if (isSearch)
			{
				SearchViewResponse[] responses =
					_viewRepository.RetrieveViewsByContextArtifactIDForSearch(
						caseContextArtifactID
					);
				dataSet = responses.ToDataSet();
			}
			else
			{
				ViewResponse[] responses =
					_viewRepository.RetrieveViewsByContextArtifactID(
						caseContextArtifactID,
						artifactTypeID
					);
				dataSet = responses.ToDataSet();
			}
			return dataSet;
		}

		public void Dispose()
		{
		}

		private static ViewFieldInfo ToViewFieldInfo(ViewFieldResponse viewFieldResponse)
		{
			global::Relativity.DataExchange.Service.ViewFieldInfo coreViewFieldInfo = new CoreViewFieldInfo(viewFieldResponse);
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