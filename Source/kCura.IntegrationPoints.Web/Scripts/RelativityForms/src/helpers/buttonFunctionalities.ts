import { IConvenienceApi } from "../types/convenienceApi";

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