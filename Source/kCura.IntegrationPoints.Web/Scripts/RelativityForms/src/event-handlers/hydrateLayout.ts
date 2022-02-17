import { contextProvider } from "../helpers/contextProvider";
import { IConvenienceApi } from "../types/convenienceApi";

export function setFieldsValues(layoutData, convenienceApi: IConvenienceApi) {
    var dest = convenienceApi.fieldHelper.getValue("Source Details"); // previously named "Destination Configuration"
    var sour = convenienceApi.fieldHelper.getValue("Source Workspace"); // previously named "Source Configuration"
    Promise.all([dest, sour]).then((values) => {
        var destConf = JSON.parse(values[0]);
        var sourConf = JSON.parse(values[1]);

        console.log(destConf);
        console.log(sourConf);

        var sourceDetails = getSourceDetails(sourConf);
        var sourceWorkspace = sourConf["SourceWorkspace"];
        var sourceRelInstance = sourConf["SourceRelativityInstance"];
        var transferedObject = destConf["ArtifactTypeName"];
        var destinationWorkspace = sourConf["TargetWorkspace"];
        var destinationFolder = sourConf["TargetFolder"];
        var multiSelectOverlay = destConf["FieldOverlayBehavior"];
        var useFolderPathInfo = formatToYesOrNo(convertToBool(destConf["UseDynamicFolderPath"]));
        var moveExistingDocs = formatToYesOrNo(convertToBool(destConf["MoveExistingDocuments"])); 
        let exportType = getExportType(convertToBool(destConf["importNativeFile"]), convertToBool(destConf["ImageImport"]));

        convenienceApi.fieldHelper.setValue("Source Details", sourceDetails);
        convenienceApi.fieldHelper.setValue("Source Workspace", sourceWorkspace);
        convenienceApi.fieldHelper.setValue("Source Rel. Instance", sourceRelInstance);
        convenienceApi.fieldHelper.setValue("Transferred Objects", transferedObject);
        convenienceApi.fieldHelper.setValue("Destination Workspace", destinationWorkspace);
        convenienceApi.fieldHelper.setValue("Destination Folder", destinationFolder);
        convenienceApi.fieldHelper.setValue("Multi-Select Overlay", multiSelectOverlay);
        convenienceApi.fieldHelper.setValue("Use Folder Path Info", useFolderPathInfo);
        convenienceApi.fieldHelper.setValue("Move Existing Docs", moveExistingDocs);
        convenienceApi.fieldHelper.setValue("Export Type", exportType);

        convenienceApi.fieldHelper.setValue("Total of Documents", "INFO NEEDED");
        convenienceApi.fieldHelper.setValue("Create Saved Search", "INFO NEEDED");
    })
}

function getSourceDetails(sourConf) {
    let sourceDetails;
    if (sourConf["SourceProductionId"]) {
        sourceDetails = "Production Set: " + sourConf["SourceProductionName"];
    } else {
        sourceDetails = "Saved Search: " + sourConf["SavedSearch"];
    }
    return sourceDetails;
}

function getExportType(importNatives: boolean, importImages: boolean) {
    let exportType = "Workspace;";
    var images = importImages ? "Images;" : "";
    var natives = importNatives ? "Natives;" : "";
    return exportType + images + natives;
}

function convertToBool(value) {
    return value === "true";
}

function formatToYesOrNo(value) {
    return convertToBool(value) ? "Yes" : "No";
};