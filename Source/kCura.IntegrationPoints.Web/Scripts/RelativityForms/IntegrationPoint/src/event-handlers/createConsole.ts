import { contextProvider } from "../helpers/contextProvider";
import { IConvenienceApi } from "../types/convenienceApi";

export function createConsole(convenienceApi: IConvenienceApi): void {
    return contextProvider((ctx) => {
        var consoleApi = convenienceApi.console;
        var integrationPointId = ctx.artifactId;
        var workspaceId = ctx.workspaceId;

        var consoleContent = generateDefaultConsoleContent(convenienceApi, ctx, workspaceId, integrationPointId);

        return consoleApi.destroy().then(function () {
            return consoleApi.containersPromise;
        }).then(function (containers) {
            containers.rootElement.appendChild(consoleContent);
        });
    })
}

function generateDefaultConsoleContent(convenienceApi, ctx, workspaceId, integrationPointId) {
    var buttonState = {
        runButtonEnabled: true,
        stopButtonEnabled: true,
        retryErrorsButtonEnabled: true,
        retryErrorsButtonVisible: true,
        viewErrorsLinkEnabled: true,
        viewErrorsLinkVisible: true,
        saveAsProfileButtonVisible: true,
        downloadErrorFileLinkEnabled: true,
        downloadErrorFileLinkVisible: true
    };

    var consoleApi = convenienceApi.console;

    var integrationPoint = {};
    ctx.fieldNameToFieldIdMap.forEach(function(value, key)
    {
       var trimmedKey = key.replaceAll(" ", "");
       try {
         var val = JSON.parse(ctx.backingModelData[value]);
       } catch(e) {
         var val = ctx.backingModelData[value];
       }
       integrationPoint[trimmedKey] = val;
    });

    function postJobAPIRequest(workspaceId, integrationPointId, action = "") {
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

    function postCreateIntegrationPointProfileRequest(workspaceId, integrationPointId, integrationPointProfileName) {
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

    function prepareGetImportProviderDocumentAPIRequest(workspaceId, integrationPointId, action) {
        return {
            options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
                headers: {
                    "content-type": "application/json; charset=utf-8"
                }
            }),
            url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/ImportProviderDocument/" + action + '?artifactId=' + integrationPointId + '&workspaceId=' + workspaceId
        };
    }

    function createRunButton(enabled) {
        return consoleApi.generate.button({
            innerText: "Run",
            disabled: !enabled,
            onclick: function() {
                function generateRunMessage() {
                    return "Are you sure you want to run this job now?";
                }

                return convenienceApi.modalService.confirm({
                    title: "Run",
                    message: generateRunMessage(),
                    acceptText: "Ok",
                    cancelText: "Cancel",
                    acceptAction: function () {
                        return postJobAPIRequest(workspaceId, integrationPointId)
                            .then(function(result) {
                                if(!result.ok) {
                                    console.log(result);
                                    return ctx.setErrorSummary(["Failed to submit integration job. Check Errors tab for details."]);
                                }
                            });
                    }
                });
            }
        });
    }

    function createStopButton(enabled) {
        return consoleApi.generate.button({
            innerText: "Stop",
            disabled: !enabled,
            onclick: function () {
                return convenienceApi.modalService.confirm({
                    title: "Stop Transfer",
                    message: "Stopping this transfer will not remove any data that was transferred. When re-running this transfer, make sure that your overwrite settings will return expected results.",
                    acceptText: "Stop Transfer",
                    cancelText: "Cancel",
                    acceptAction: function () {
                        return postJobAPIRequest(workspaceId, integrationPointId, "Stop")
                            .then(function(result) {
                                if(!result.ok) {
                                    console.log(result);
                                    return ctx.setErrorSummary(["Failed to stop the job. Check Errors tab for details."]);
                                }
                            });
                    }
                })
            }
        });
    }

    function createRetryErrorsButton(enabled) {
        return consoleApi.generate.button({
            innerText: "Retry Errors",
            disabled: !enabled,
            onclick: function () { 
                console.log("Retry Errors Clicked!");

                function generateRunMessage() {
                    return "Are you sure you want to run this job now?";
                }

                return convenienceApi.modalService.confirm({
                    title: "Retry Errors",
                    message: generateRunMessage(),
                    acceptText: "Ok",
                    cancelText: "Cancel",
                    acceptAction: function () {
                         return postJobAPIRequest(workspaceId, integrationPointId, "Retry")
                            .then(function(result) {
                                if(!result.ok) {
                                    console.log(result);
                                    return ctx.setErrorSummary(["Failed to submit retry job. Check Errors tab for details."]);
                                }
                            });
                    }
                });
            }
        });
    }

    function createViewErrorsLink(enabled) {
        return consoleApi.generate.buttonStyledAsLink({
            innerText: "View Errors",
            disabled: !enabled,
            onclick: function () { console.log("View Errors Clicked!"); }
        });
    }

    function createSaveAsProfileButton() {
        return consoleApi.generate.button({
            innerText: "Save as a Profile",
            onclick: function (e) {

                // this content container should be edited, right now its not looking great - but works
                var contentContainer = document.createElement("div");
                contentContainer.innerHTML = "<input type='text' value='' id='inputIntegrationPointProfileName'/>";

                var model = {
                    title: "Save Integration Point as Profile",
                    theme: "confirmation",
                    contentElement: contentContainer,
                    actions: [
                        {
                            text: "Ok",
                            click: function click() {
                                // @ts-ignore
                                model.accept("Accept payload");
                            }
                        },
                        {
                            text: "Cancel",
                            click: function click() {
                                // @ts-ignore
                                model.cancel("Cancel payload");
                            }
                        }
                    ],
                    acceptAction: function () {
                         var name = (<HTMLInputElement>document.getElementById('inputIntegrationPointProfileName')).value;
                         return postCreateIntegrationPointProfileRequest(workspaceId, integrationPointId, name)
                             .then(function (result) {
                                 if (!result.ok) {
                                     console.log(result);
                                     return ctx.setErrorSummary(["Failed to create integration profile, try again or lose hope"]);
                                 }
                             });
                    }
                };

                return convenienceApi.modalService.openCustomModal(model).then(function (closeResult) {
                    if (closeResult.wasCancelled) {
                        console.log("Create integration point profile modal was cancelled");
                    } else {
                        console.log("Create integration point profile modal was accepted");
                    }
                    console.log(closeResult);
                });
            }
        });
    }

    function createDownloadErrorFileLink(enabled) {
        return consoleApi.generate.buttonStyledAsLink({
            innerText: "Download Error File",
            disabled: !enabled,
            onclick: function (e) {
                var request = prepareGetImportProviderDocumentAPIRequest(workspaceId, integrationPointId, "CheckErrorFile");

                return convenienceApi.relativityHttpClient.get(request.url, request.options)
                    .then(function(result) {
                        if(!result.ok) {
                            console.log(result);
                            return ctx.setErrorSummary(["The error file could not be found for download. Check Errors tab for details."]);
                        }

                        var request = prepareGetImportProviderDocumentAPIRequest(workspaceId, integrationPointId, "DownloadErrorFile");
                        window.location.assign(request.url);
                    });
            }
        });
    }

    var transferOptionsTitle = consoleApi.generate.sectionTitle({
        innerText: "Transfer Options"
    });

    var transferSection = [transferOptionsTitle];

    if (buttonState.runButtonEnabled) {
        var runButton = createRunButton(buttonState.runButtonEnabled);
        transferSection.push(runButton);
    }

    if (buttonState.stopButtonEnabled) {
        var stopButton = createStopButton(buttonState.runButtonEnabled);
        transferSection.push(stopButton);
    }

    if (buttonState.retryErrorsButtonVisible) {
        var retryErrorsButton = createRetryErrorsButton(buttonState.retryErrorsButtonEnabled);
        transferSection.push(retryErrorsButton);
    }

    if (buttonState.viewErrorsLinkVisible) {
        var viewErrorsLink = createViewErrorsLink(buttonState.viewErrorsLinkEnabled);
        transferSection.push(viewErrorsLink);
    }

    if (buttonState.saveAsProfileButtonVisible) {
        var saveAsProfileButton = createSaveAsProfileButton();
        transferSection.push(saveAsProfileButton);
    }

    if (buttonState.downloadErrorFileLinkVisible) {
        var downloadErrorFileLink = createDownloadErrorFileLink(buttonState.downloadErrorFileLinkEnabled);
        transferSection.push(downloadErrorFileLink);
    }

    var transferOptionsSection = consoleApi.generate.section({}, transferSection);

    return transferOptionsSection;
}