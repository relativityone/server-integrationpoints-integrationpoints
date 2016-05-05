using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Service;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Process
{
	internal class ExportProcessBuilder
	{
		public Exporter Create(ExportSettings settings)
		{
			ExportFile exportFile = ExportFileHelper.CreateDefaultSetup(settings);
			PerformLogin(exportFile);
			PopulateExportFieldsSettings(exportFile, settings.SelViewFieldIds);
			return new Exporter(exportFile, new Controller());
		}

		private void PerformLogin(ExportFile exportSettings)
		{
			exportSettings.CookieContainer = new CookieContainer();
			exportSettings.Credential = WinEDDS.Api.LoginHelper.LoginWindowsAuth(exportSettings.CookieContainer);
		}

		private void PopulateExportFieldsSettings(ExportFile exportFile, List<int> selectedViewFieldIds)
		{
			using (SearchManager searchManager = new SearchManager(exportFile.Credential, exportFile.CookieContainer))
			using (CaseManager caseManager = new CaseManager(exportFile.Credential, exportFile.CookieContainer))
			{
				PopulateCaseInfo(exportFile, caseManager);
				PopulateViewFields(exportFile, selectedViewFieldIds, searchManager);
			}
		}

		private static void PopulateViewFields(ExportFile exportFile, List<int> selectedViewFieldIds, SearchManager searchManager)
		{
			exportFile.AllExportableFields =
				searchManager.RetrieveAllExportableViewFields(exportFile.CaseInfo.ArtifactID, exportFile.ArtifactTypeID);

			exportFile.SelectedViewFields = exportFile.AllExportableFields
				.Where(item => selectedViewFieldIds.Any(selViewFieldId => selViewFieldId == item.FieldArtifactId))
				.ToArray();
		}

		private static void PopulateCaseInfo(ExportFile exportFile, CaseManager caseManager)
		{
			if (string.IsNullOrEmpty(exportFile.CaseInfo.DocumentPath))
			{
				exportFile.CaseInfo = caseManager.Read(exportFile.CaseInfo.ArtifactID);
			}
		}
	}
}
