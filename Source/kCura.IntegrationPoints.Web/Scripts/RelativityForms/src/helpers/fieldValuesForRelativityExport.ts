import { IConvenienceApi } from "../types/convenienceApi";

export async function getNativesStats(convenienceApi: IConvenienceApi, workspaceId: number, savedSearchId: number) {
    let request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            workspaceId: workspaceId,
            savedSearchId: savedSearchId
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/SummaryPage/GetNativesStatisticsForSavedSearch"
    };

    let resp = convenienceApi.relativityHttpClient.post(request.url, request.payload, request.options)
        .then(function (result) {
            if (!result.ok) {
                console.log("error in get; ", result);
            } else if (result.ok) {
                return result.json();
            }
        });
    return resp;
}

export async function getImagesStatsForSavedSearch(convenienceApi: IConvenienceApi, workspaceId: number, savedSearchId: number, importNatives: boolean) {
    let request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            workspaceId: workspaceId,
            savedSearchId: savedSearchId,
            calculateSize: importNatives
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/SummaryPage/GetImagesStatisticsForSavedSearch"
    };

    let resp = convenienceApi.relativityHttpClient.post(request.url, request.payload, request.options)
        .then(function (result) {
            if (!result.ok) {
                console.log("error in get; ", result);
            } else if (result.ok) {
                return result.json();
            }
        });
    return resp;
}

export async function getImagesStatsForProduction(convenienceApi: IConvenienceApi, workspaceId: number, productionId: number) {
    let request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            workspaceId: workspaceId,
            productionId: productionId
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/SummaryPage/GetImagesStatisticsForProduction"
    };

    let resp = convenienceApi.relativityHttpClient.post(request.url, request.payload, request.options)
        .then(function (result) {
            if (!result.ok) {
                console.log("error in get; ", result);
            } else if (result.ok) {
                return result.json();
            }
        });
    return resp;
}

export function prepareStatsInfo(total, size) {
    let result = "";
    if (total > -1) {
        result += total;
        if (size > -1) {
            result += " (" + formatBytes(size) + ")";
        } else if (size === -1) {
            result += " (Error occurred)";
        } else {
            result += " (Calculating size...)";
        }
    } else if (total === -1) {
        result = "Error occurred";
    }
    return result;
}

function formatBytes(bytes) {
    if (bytes === 0) return '0 Bytes';
    let k = 1024;
    let sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    let i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

export function getExportType(sourceConfiguration: Object, destinationConfiguration: Object) {
    // if source configuration has an "ExportType" property, then it's export to loadfile - othervise export to relativity
    if (sourceConfiguration["ExportType"]) {
        return "Load file; "
            + (sourceConfiguration["ExportImages"] ? "Images" : "")
            + (sourceConfiguration["ExportNatives"] ? "Natives" : "")
            + (sourceConfiguration["ExportFullTextAsFile"] ? "Text As Files" : "");
    } else {
        return "Workspace; "
            + (convertToBool(destinationConfiguration["ImageImport"]) ? "Images" : "")
            + (convertToBool(destinationConfiguration["importNativeFile"]) ? "Natives" : "")
            + (destinationConfiguration["ArtifactTypeName"] !== "Document" ? "View" : "");
    }
}

export function getSourceDetails(sourceConfiguration: Object) {

    if (sourceConfiguration["SourceProductionId"]) {
        return "Production Set: " + sourceConfiguration["SourceProductionName"];
    } else if (sourceConfiguration["SavedSearchArtifactId"]) {
        return "Saved Search: " + sourceConfiguration["SavedSearch"];
    } else if (sourceConfiguration["SourceViewId"]) {
        return "View: " + sourceConfiguration["ViewName"];
    }

    switch (sourceConfiguration["ExportType"]) {
        case 0:
            return "Folder: " + sourceConfiguration["FolderArtifactName"];
        case 1:
            return "Folder + Subfolders: " + sourceConfiguration["FolderArtifactName"];
        case 2:
            return "Production: " + sourceConfiguration["ProductionName"];
        case 3:
            return "Saved search: " + sourceConfiguration["SavedSearch"];
    }
}

export function getPrecenenceSummary(destinationConfiguration: Object) {
    let productionPrecedence = (destinationConfiguration["ProductionPrecedence"] === 0 ? "Original" : "Produced");
    let imagePrecedence = destinationConfiguration["ImagePrecedence"];
    if (imagePrecedence && imagePrecedence.length > 0 ) {
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