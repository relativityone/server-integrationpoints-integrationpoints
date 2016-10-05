using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using kCura.WinEDDS;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Authentication;
using kCura.IntegrationPoints.ImportProvider.Parser.Services.Interfaces;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.ImportProvider.Parser.Services
{
    public class ImportPreviewService : IImportPreviewService
    {
        private ICredentialProvider _credentialProvider;

        public ImportPreviewService(ICredentialProvider credentialProvider)
        {
            _credentialProvider = credentialProvider;
        }
        
        public ImportPreviewTable PreviewLoadFile(string filePath, int workspaceID)
        {
            //Set up Config object with WebAPI link
            var webApiConfig = new WebApiConfig();
            WinEDDS.Config.WebServiceURL = webApiConfig.GetWebApiUrl;

            var cookeiContainer = new System.Net.CookieContainer();
            var factory = new NativeSettingsFactory(_credentialProvider.Authenticate(cookeiContainer), workspaceID);
            var eddsLoadFile = factory.ToLoadFile();

            eddsLoadFile.RecordDelimiter = ',';

            eddsLoadFile.FilePath = filePath;
            eddsLoadFile.LoadNativeFiles = false;
            eddsLoadFile.CreateFolderStructure = false;

            var temp = new LoadFileReader(eddsLoadFile, false);

            //set up field mapping to extract all fields with reader
            var columns = temp.GetColumnNames(eddsLoadFile);
            int colIdx = 0;
            foreach (string colName in columns)
            {
                int fieldCat = -1;
                //setting the first column as the identifier
                //TODO: we should do this based on the mapping
                if (colIdx == 0) { fieldCat = 2; }

                //TODO: We need to look at the val of colName and see if it's mapped.
                //If Not, we don't need to insert this guy into the FieldMap
                //If it is, we should insert but create the DocumentField object to have the Name of the Source Field 
                var newDocField = new DocumentField(colName, colIdx, 4, fieldCat, -1, -1, -1, false, kCura.EDDS.WebAPI.DocumentManagerBase.ImportBehaviorChoice.LeaveBlankValuesUnchanged, false);

                var newfieldMapItem = new LoadFileFieldMap.LoadFileFieldMapItem(newDocField, colIdx);
                eddsLoadFile.FieldMap.Add(newfieldMapItem);
                colIdx++;                
            }
            
            //instantiate with errorsOnly and doRetryLogic == FALSE
            var previewer = new kCura.WinEDDS.LoadFilePreviewer(eddsLoadFile, 0, false, false);
            ArrayList arrs = (ArrayList)previewer.ReadFile(filePath, 0);

            ImportPreviewTable preview = new ImportPreviewTable();
            preview.Header = (arrs[0] as kCura.WinEDDS.Api.ArtifactField[]).Select(i => i.DisplayName).ToList();
            foreach (kCura.WinEDDS.Api.ArtifactField[] item in arrs)
            {
                List<string> row = item.Select(i => i.Value.ToString()).ToList();
                preview.Data.Add(row);
            }

            return preview;
        }

        public ImportPreviewTable PreviewErrors()
        {
            throw new NotImplementedException();
        }

        public ImportPreviewTable PreviewChoicesFolders()
        {
            throw new NotImplementedException();
        }
    }    
}
