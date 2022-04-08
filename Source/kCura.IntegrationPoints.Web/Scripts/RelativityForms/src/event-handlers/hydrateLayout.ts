import { getConnectionAuthenticationType, getFilter } from "../helpers/fieldValuesForImport";
import { getDestinationDetails, getFilePath, getImageFileFormat, getImageFileType, getImagePrecedence, getImportType, getLoadFileFormat, getPrecedenceList, getSubdirectoryInfo, getTextAndNativeFileNames, getTextFileEncoding, getVolume } from "../helpers/fieldValuesForLoadFileExport";
import { formatToYesOrNo, getExportType, getImagesStatsForProduction, getImagesStatsForSavedSearch, getNativesStats, getPrecenenceSummary, getSourceDetails, prepareStatsInfo } from "../helpers/fieldValuesForRelativityExport";
import { IConvenienceApi } from "../types/convenienceApi";

export function setFieldsValues(layoutData, convenienceApi: IConvenienceApi, sourceConfiguration: Object, destinationConfiguration: Object) {

    var sourceDetails = getSourceDetails(sourceConfiguration);
    var useFolderPathInfo = formatToYesOrNo(destinationConfiguration["UseDynamicFolderPath"]);
    let exportType = getExportType(sourceConfiguration, destinationConfiguration);

    // export 
    convenienceApi.fieldHelper.setValue("Export Type", exportType);
    convenienceApi.fieldHelper.setValue("Source Details", sourceDetails);

    // export to relativity
    convenienceApi.fieldHelper.setValue("Source Workspace", sourceConfiguration["SourceWorkspace"]);
    convenienceApi.fieldHelper.setValue("Source Rel. Instance", sourceConfiguration["SourceRelativityInstance"]);
    convenienceApi.fieldHelper.setValue("Transfered Object", destinationConfiguration["ArtifactTypeName"]);
    convenienceApi.fieldHelper.setValue("Destination Workspace", sourceConfiguration["TargetWorkspace"]);
    convenienceApi.fieldHelper.setValue("Destination Folder", sourceConfiguration["TargetFolder"]);
    convenienceApi.fieldHelper.setValue("Destination Production Set", sourceConfiguration["targetProductionSet"]);
    convenienceApi.fieldHelper.setValue("Multi-Select Overlay", destinationConfiguration["FieldOverlayBehavior"]);
    convenienceApi.fieldHelper.setValue("Use Folder Path Info", useFolderPathInfo);
    convenienceApi.fieldHelper.setValue("Move Existing Docs", formatToYesOrNo(destinationConfiguration["MoveExistingDocuments"]));
    convenienceApi.fieldHelper.setValue("Image Precedence", getPrecenenceSummary(destinationConfiguration));
    convenienceApi.fieldHelper.setValue("Copy Files to Repository", formatToYesOrNo(destinationConfiguration["importNativeFile"]));

    // import from load file
    convenienceApi.fieldHelper.setValue("Destination RDO", destinationConfiguration["ArtifactTypeName"]);
    convenienceApi.fieldHelper.setValue("Source Location", sourceConfiguration["LoadFile"]);
    convenienceApi.fieldHelper.setValue("Import Type", getImportType(sourceConfiguration["ImportType"]));

    // export to load file 
    getDestinationDetails(sourceConfiguration, convenienceApi).then(label => {
        convenienceApi.fieldHelper.setValue("Destination Details", label);
    })

    convenienceApi.fieldHelper.setValue("Overwrite Files", formatToYesOrNo(destinationConfiguration["OverwriteFiles"]));
    convenienceApi.fieldHelper.setValue("Start at record", sourceConfiguration["StartExportAtRecord"]);
    convenienceApi.fieldHelper.setValue("Volume", getVolume(sourceConfiguration));
    convenienceApi.fieldHelper.setValue("Subdirectory", getSubdirectoryInfo(sourceConfiguration));
    getLoadFileFormat(
        convenienceApi,
        sourceConfiguration["SourceWorkspaceArtifactId"],
        sourceConfiguration["DataFileEncodingType"],
        sourceConfiguration["SelectedDataFileFormat"]).then(label => {
            convenienceApi.fieldHelper.setValue("Load file format", label);
        })

    convenienceApi.fieldHelper.setValue("File path", getFilePath(sourceConfiguration["FilePath"], sourceConfiguration["IncludeNativeFilesPath"], sourceConfiguration["UserPrefix"]));
    convenienceApi.fieldHelper.setValue("Text and Native File Names", getTextAndNativeFileNames(sourceConfiguration["ExportNativesToFileNamedFrom"], sourceConfiguration["AppendOriginalFileName"], sourceConfiguration["FileNameParts"]));
    convenienceApi.fieldHelper.setValue("Image file format", getImageFileFormat(sourceConfiguration["ExportImages"], sourceConfiguration["SelectedImageDataFileFormat"]));
    convenienceApi.fieldHelper.setValue("Image file type", getImageFileType(sourceConfiguration["ExportImages"], sourceConfiguration["SelectedImageFileType"]));
    convenienceApi.fieldHelper.setValue("Image precedence", getImagePrecedence(sourceConfiguration["ExportImages"], sourceConfiguration["ProductionPrecedence"], sourceConfiguration["ImagePrecedence"]));
    convenienceApi.fieldHelper.setValue("Text precedence", getPrecedenceList(sourceConfiguration));
    getTextFileEncoding(
        convenienceApi,
        sourceConfiguration["SourceWorkspaceArtifactId"],
        sourceConfiguration["TextFileEncodingType"]
    ).then(label => {
        convenienceApi.fieldHelper.setValue("Text file encoding", label);
    })
    convenienceApi.fieldHelper.setValue("Multiple choice as nested", (sourceConfiguration["ExportMultipleChoiceFieldsAsNested"] ? "Yes" : "No"));

    // import from LDAP
    convenienceApi.fieldHelper.setValue("Connection Path", sourceConfiguration["connectionPath"]);
    convenienceApi.fieldHelper.setValue("Object Filter String", getFilter(sourceConfiguration["filter"]));
    convenienceApi.fieldHelper.setValue("Authentication", getConnectionAuthenticationType(sourceConfiguration["connectionAuthenticationType"]));
    convenienceApi.fieldHelper.setValue("Import Nested Items", formatToYesOrNo(sourceConfiguration["importNested"]));

    // import from FTP
    convenienceApi.fieldHelper.setValue("Host", sourceConfiguration["Host"]);
    convenienceApi.fieldHelper.setValue("Port", sourceConfiguration["Port"]);
    convenienceApi.fieldHelper.setValue("Protocol", sourceConfiguration["Protocol"]);
    convenienceApi.fieldHelper.setValue("Filename Prefix", sourceConfiguration["FileNamePrefix"]);
    convenienceApi.fieldHelper.setValue("Timezone Offset", sourceConfiguration["TimezoneOffset"]);

    if (sourceConfiguration["SourceViewId"] != 10) {
        if (sourceConfiguration["SourceProductionId"]) {
            getImagesStatsForProduction(convenienceApi, sourceConfiguration["SourceWorkspaceArtifactId"], sourceConfiguration["SourceProductionId"]).then(data => {
                convenienceApi.fieldHelper.setValue("Total of Documents", data["DocumentsCount"]);
                convenienceApi.fieldHelper.setValue("Total of Images", prepareStatsInfo(data["TotalImagesCount"], data["TotalImagesSizeBytes"]));
            })
        } else if (destinationConfiguration["importNativeFile"] == 'true' && !importImageFiles(destinationConfiguration)) {
            getNativesStats(convenienceApi, sourceConfiguration["SourceWorkspaceArtifactId"], sourceConfiguration["SavedSearchArtifactId"]).then(data => {
                convenienceApi.fieldHelper.setValue("Total of Documents", data["DocumentsCount"]);
                convenienceApi.fieldHelper.setValue("Total of Natives", prepareStatsInfo(data["TotalNativesCount"], data["TotalNativesSizeBytes"]));
            })
        } else {
            getImagesStatsForSavedSearch(convenienceApi, sourceConfiguration["SourceWorkspaceArtifactId"], sourceConfiguration["SavedSearchArtifactId"], (destinationConfiguration["getImagesStatsForProduction"] === 'true')).then(data => {
                convenienceApi.fieldHelper.setValue("Total of Documents", data["DocumentsCount"]);
                convenienceApi.fieldHelper.setValue("Total of Images", prepareStatsInfo(data["TotalImagesCount"], data["TotalImagesSizeBytes"]));
            })
        }

        convenienceApi.fieldHelper.setValue("Create Saved Search", formatToYesOrNo(destinationConfiguration["CreateSavedSearchForTagging"]));
    }
}

function importImageFiles(destinationConfiguration: Object) {
    return (destinationConfiguration["ImageImport"] == 'true' && (!destinationConfiguration["ImagePrecedence"] || destinationConfiguration["ImagePrecedence"].length == 0));
}