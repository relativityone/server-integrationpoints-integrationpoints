import { IConvenienceApi } from "../types/convenienceApi";
import { formatToYesOrNo, getExportType, getFolderPathInformation, getImagesStatsForProduction, getImagesStatsForSavedSearch, getNativesStats, getPrecenenceSummary, getSourceDetails, prepareStatsInfo } from "../helpers/fieldValuesForRelativityExport";

export function postJobAPIRequest(convenienceApi: IConvenienceApi, workspaceId, integrationPointId, action = "") {
    var request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            appId: workspaceId,
            artifactId: integrationPointId
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/Job/" + action
    };

    return convenienceApi.relativityHttpClient.post(request.url, request.payload, request.options)
}

export function calculateStatsRequest(convenienceApi: IConvenienceApi, sourceConfiguration, destinationConfiguration) {

    convenienceApi.fieldHelper.setValue("Total of Documents", "Calculating...(leaving this page will cancel calculation)");
    convenienceApi.fieldHelper.setValue("Total of Images", "Calculating...(leaving this page will cancel calculation)");
    convenienceApi.fieldHelper.setValue("Total of Documents", "Calculating...(leaving this page will cancel calculation)");
    convenienceApi.fieldHelper.setValue("Total of Natives", "Calculating...(leaving this page will cancel calculation)");

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
}

export function postCreateIntegrationPointProfileRequest(convenienceApi: IConvenienceApi, workspaceId, integrationPointId, integrationPointProfileName) {
    var request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        payload: {
            integrationPointArtifactId: integrationPointId,
            profileName: integrationPointProfileName
        },
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/IntegrationPointProfilesAPI/SaveAsProfile/"
    }

    return convenienceApi.relativityHttpClient.post(request.url, request.payload, request.options)
}

export function prepareGetViewErrorsPath(convenienceApi: IConvenienceApi, workspaceId, integrationPointId) {
    var request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/Error/GetViewErrorsLink" + '?integrationPointId=' + integrationPointId + '&workspaceId=' + workspaceId
    }

    return request;
}

export function prepareGetImportProviderDocumentAPIRequest(convenienceApi: IConvenienceApi, workspaceId, integrationPointId, action) {
    return {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/ImportProviderDocument/" + action + '?artifactId=' + integrationPointId + '&workspaceId=' + workspaceId
    };
}

function importImageFiles(destinationConfiguration: Object) {
    return (destinationConfiguration["ImageImport"] == 'true' && (!destinationConfiguration["ImagePrecedence"] || destinationConfiguration["ImagePrecedence"].length == 0))
};