import { IConvenienceApi } from "../types/convenienceApi";

export const enum calculationType {
    ImagesStatsForProduction = 1,
    NativesStats = 2,
    ImagesStatsForSavedSearch = 3
}

export async function getFolderPathInformation(convenienceApi: IConvenienceApi, workspaceId: number, destinationConfiguration: object) {
    if (convertToBool(destinationConfiguration["UseFolderPathInformation"])) {
        let request = {
            options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
                headers: {
                    "content-type": "application/json; charset=utf-8"
                }
            }),
            url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/FolderPath/GetFields"
        };

        return convenienceApi.relativityHttpClient.get(request.url, request.options)
            .then(function (result) {
                if (!result.ok) {
                    console.log("error in get; ", result);
                }
                return result.json();
            }).then(result => {
                var field = result.find(field => field.fieldIdentifier === destinationConfiguration["FolderPathSourceField"]);
                if (field) {
                    return "Read From Field: " + field.actualName;
                }
            })
    } else if (convertToBool(destinationConfiguration["UseDynamicFolderPath"])) {
        return "Read From Folder Tree";
    } else {
        return "No";
    }
}

export function getCalculationStateInfo(convenienceApi: IConvenienceApi, workspaceId: number, integrationPointId: number) {
    let request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            workspaceId: workspaceId,
            integrationPointId: integrationPointId
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/SummaryPage/GetCalculationStateInfo"
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

export async function getNativesStats(convenienceApi: IConvenienceApi, workspaceId: number, savedSearchId: number, integrationPointId: number) {
    let request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            workspaceId: workspaceId,
            savedSearchId: savedSearchId,
            integrationPointId: integrationPointId
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/SummaryPage/GetNativesStatisticsForSavedSearch"
    };

    return sendStatsRequestAndHandleResponse(convenienceApi, request);   
}

export async function getImagesStatsForSavedSearch(convenienceApi: IConvenienceApi, workspaceId: number, savedSearchId: number, importNatives: boolean, integrationPointId: number) {
    let request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            workspaceId: workspaceId,
            savedSearchId: savedSearchId,
            calculateSize: importNatives,
            integrationPointId: integrationPointId
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/SummaryPage/GetImagesStatisticsForSavedSearch"        
    };   

    return sendStatsRequestAndHandleResponse(convenienceApi, request);   
}

export async function getImagesStatsForProduction(convenienceApi: IConvenienceApi, workspaceId: number, productionId: number, integrationPointId: number) {
    let request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            workspaceId: workspaceId,
            productionId: productionId,
            integrationPointId: integrationPointId
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/SummaryPage/GetImagesStatisticsForProduction"
    };

    return sendStatsRequestAndHandleResponse(convenienceApi, request);   
}

function sendStatsRequestAndHandleResponse(convenienceApi, request) {
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

function prepareStatsInfo(total, size) {
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

export function handleStatistics(convenienceApi, data, type) {
    var documentsLabelName = "";
    var nativesOrImagesLabelName = "";
    var totalCountFieldName = "";
    var totalSizeFieldName = "";

    if (type == calculationType.NativesStats) {
        documentsLabelName = "Total of Documents";
        nativesOrImagesLabelName = "Total of Natives";
        totalCountFieldName = "TotalNativesCount";
        totalSizeFieldName = "TotalNativesSizeBytes";
    }

    // TODO: for now there is no difference in label naming, but we need to split conditions for ImagesStatsForProduction and ImagesStatsForSavedSearch enum in REL-786615
    if (type == calculationType.ImagesStatsForProduction || type == calculationType.ImagesStatsForSavedSearch) {
        documentsLabelName = "Total of Documents";
        nativesOrImagesLabelName = "Total of Images";
        totalCountFieldName = "TotalImagesCount";
        totalSizeFieldName = "TotalImagesSizeBytes";
    }

   if (data === 'undefined' || data["Status"] == 4) {
        // calculation ended with errors
        convenienceApi.fieldHelper.setValue(documentsLabelName, "Error occurred");
        convenienceApi.fieldHelper.setValue(nativesOrImagesLabelName, "Error occurred");
    }
    else {
        // calculation completed
        var stats = data["DocumentStatistics"];
        var lastCalculationDate = "Calculated on: " + stats["CalculatedOn"] + " UTC";
        var valueToDisplay = `${stats["DocumentsCount"]}
${lastCalculationDate}`;

        convenienceApi.fieldHelper.setValue(documentsLabelName, valueToDisplay);

        var total = prepareStatsInfo(stats[totalCountFieldName], stats[totalSizeFieldName]);
        convenienceApi.fieldHelper.setValue(nativesOrImagesLabelName, total);
    }
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
    if (imagePrecedence && imagePrecedence.length > 0) {
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