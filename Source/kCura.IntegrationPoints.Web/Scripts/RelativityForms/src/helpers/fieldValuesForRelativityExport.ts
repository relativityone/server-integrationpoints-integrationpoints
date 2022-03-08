export function getExportType(sourceConfiguration: Object, destinationConfiguration: Object) {
    // if source configuration has an "ExportType" property, then it's export to loadfile - othervise export to relativity
    if (sourceConfiguration["ExportType"]) {
        return "Load file"
            + (sourceConfiguration["ExportImages"] ? "; Images" : "")
            + (sourceConfiguration["ExportNatives"] ? "; Natives" : "")
            + (sourceConfiguration["ExportFullTextAsFile"] ? "; Text As Files" : "");
    } else {
        return "Workspace"
            + (convertToBool(destinationConfiguration["ImageImport"]) ? "; Images" : "")
            + (convertToBool(destinationConfiguration["importNativeFile"]) ? "; Natives" : "");
    }

}

export function getSourceDetails(sourceConfiguration, transferredObjects) {

    if (sourceConfiguration["SourceProductionId"]) {
        return "Production Set: " + sourceConfiguration["SourceProductionName"];
    } else if (sourceConfiguration["SavedSearchArtifactId"]) {
        return "Saved Search: " + sourceConfiguration["SavedSearch"];
    }

    if (transferredObjects !== "Document") {
        return "RDO: " + transferredObjects + "; " + sourceConfiguration["ViewName"];
    }
    if (sourceConfiguration["ExportType"] === 3) {
        return "Saved search: " + sourceConfiguration["SavedSearch"];
    }
    if (sourceConfiguration["ExportType"] === 0) {
        return "Folder: " + sourceConfiguration["FolderArtifactName"];
    }
    if (sourceConfiguration["ExportType"] === 1) {
        return "Folder + Subfolders: " + sourceConfiguration["FolderArtifactName"];
    }
    if (sourceConfiguration["ExportType"] === 2) {
        return "Production: " + sourceConfiguration["ProductionName"];
    }
}

export function getPrecenenceSummary(destinationConfiguration: Object) {
    let productionPrecedence = (destinationConfiguration["ProductionPrecedence"] === 0 ? "Original" : "Produced");
    let imagePrecedence = destinationConfiguration["ImagePrecedence"];
    if (imagePrecedence) {
        return (productionPrecedence + ": " + imagePrecedence.map(function (x) {
            return x.displayName;
        }).join("; "));
    } else {
        return productionPrecedence;
    }
}

function convertToBool(value) {
    return value === "true";
}

export function formatToYesOrNo(value) {
    return convertToBool(value) ? "Yes" : "No";
};