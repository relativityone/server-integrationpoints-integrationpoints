import { IConvenienceApi } from "../types/convenienceApi";
import { postCreateIntegrationPointProfileRequest, postJobAPIRequest, prepareGetImportProviderDocumentAPIRequest, prepareGetViewErrorsPath, calculateStatsRequest } from "./buttonFunctionalities";

export function createRunButton(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, workspaceId: number, integrationPointId: number, lqMessageContainer: Element) {
    return consoleApi.generate.button({
        innerText: "Run",
        disabled: !enabled,
        onclick: function (e) {
            function generateRunMessage() {
                return "Are you sure you want to run this job now?";
            }

            var contentContainer = document.createElement("div");
            contentContainer.innerText = generateRunMessage();

            var model = {
                title: "Run",
                theme: "confirmation",
                contentElement: contentContainer,
                actions: [
                    {
                        text: "Ok",
                        click: function click() {
                            contentContainer.innerHTML = '<rwc-leaderboard loading loading-header="Submitting your job..." loading-information="Please wait a few seconds..."></rwc-leaderboard>'
                            model.actions = [];

                            let promise = postJobAPIRequest(convenienceApi, workspaceId, integrationPointId);
                            promise.then(function (result) {
                                console.log(result.ok);
                                if (!result.ok) {
                                    let res = result.json();

                                    res.then(res => {
                                        let header = "Failed to submit integration job: ";
                                        let messages = '["';
                                        res.errors.forEach(x => {
                                            messages += x.message + '",';
                                        })

                                        messages = messages.slice(0, -1) + ']';

                                        createMessageContainer(messages, "error", lqMessageContainer, header);

                                        // @ts-ignore
                                        model.close("Close model");
                                    })
                                } else {
                                    createMessageContainer('["Job started!"]', "success", lqMessageContainer, "");

                                    // @ts-ignore
                                    model.accept("Accept run");
                                }
                            })
                                .catch(err => {
                                    console.log(err);
                                    // @ts-ignore
                                    model.cancel("Unable to run");
                                });
                        }
                    },
                    {
                        text: "Cancel",
                        click: function click() {
                            // @ts-ignore
                            model.cancel("Cancel run");
                        }
                    }
                ]
            };

            return convenienceApi.modalService.openCustomModal(model);
        }
    });
}

function createMessageContainer(message: string, theme: string, lqMessageContainer: Element, title: string) {
    let messageContainer = document.createElement("rwc-message-container");
    messageContainer.setAttribute("class", "RIPCustomMessageBar");
    messageContainer.setAttribute("theme", theme);
    if (theme === "error") {
        messageContainer.setAttribute("message-collection-title-prefix", title);
    }
    messageContainer.setAttribute("messages", message);
    lqMessageContainer.appendChild(messageContainer);
}

export function removeMessageContainers() {
    try {
        let messages = document.getElementsByClassName("RIPCustomMessageBar")
        for (let i = 0; i < messages.length; i++) {
            messages[i].parentNode.removeChild(messages[i])
        }
    } catch (err) {
        console.log(err)
    }

}

export function createStopButton(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, workspaceId: number, integrationPointId: number) {
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
                    return postJobAPIRequest(convenienceApi, workspaceId, integrationPointId, "Stop")
                        .then(function (result) {
                            if (!result.ok) {
                                console.log(result);
                                return ctx.setErrorSummary(["Failed to stop the job. Check Errors tab for details."]);
                            }
                        });
                }
            })
        }
    });
}

export function createCalculateStatsButton(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, integrationPointId: number) {
    return consoleApi.generate.button({
        innerText: "Get statistics",
        disabled: !enabled,
        onclick: function () {
            return convenienceApi.modalService.confirm({
                title: "Calculate statistics",
                message: "This action will launch the calculation of Saved Search content. The operation can be time consuming. Refreshing or leaving this page will cancel calculation.",
                acceptText: "Calculate",
                cancelText: "Cancel",
                acceptAction: function () {

                    let keys = Object.keys(ctx.backingModelData);
                    keys.sort((a, b) => { return Number(a) - Number(b) });
                    let sourceConfiguration;
                    try {
                        sourceConfiguration = JSON.parse(ctx.backingModelData[keys[4].toString()]);
                    } catch (e) {
                        sourceConfiguration = {
                            "SourceConfiguration": ctx.backingModelData[keys[4].toString()]
                        }
                    }
                    let destinationConfiguration = JSON.parse(ctx.backingModelData[keys[5].toString()]);

                    calculateStatsRequest(convenienceApi, sourceConfiguration, destinationConfiguration, integrationPointId);                
                }
            });
        }
    });
}

function importImageFiles(destinationConfiguration: Object) {
    return (destinationConfiguration["ImageImport"] == 'true' && (!destinationConfiguration["ImagePrecedence"] || destinationConfiguration["ImagePrecedence"].length == 0))
};

export function createRetryErrorsButton(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, workspaceId: number, integrationPointId: number, overwriteOption: string, lqMessageContainer: Element) {

    return consoleApi.generate.button({
        innerText: "Retry Errors",
        disabled: !enabled,
        onclick: function () {
            function generateRunMessage() {

                var selectedMessage = "";

                if (overwriteOption === "Append Only") {
                    selectedMessage = "Select mode for the retry job. Warning: in Append/Overlay mode document metadata with the same identifier will be overwritten in the target workspace.";
                } else if (overwriteOption === "Overlay Only") {
                    selectedMessage = "The retry job will run in Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
                } else {
                    selectedMessage = "The retry job will run in Append/Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
                }
                return selectedMessage;
            }

            let switchToAppendOverlayMode = false;

            function generateModal(switchToAppendOverlayMode: boolean) {
                contentContainer.innerHTML = '<rwc-leaderboard loading loading-header="Submitting your job..." loading-information="Please wait a few seconds..."></rwc-leaderboard>'
                model.actions = [];

                let action = 'Retry?switchToAppendOverlayMode=' + switchToAppendOverlayMode;
                let promise = postJobAPIRequest(convenienceApi, workspaceId, integrationPointId, action);
                promise.then(function (result) {
                    console.log(result.ok);
                    if (!result.ok) {
                        let res = result.json();

                        res.then(res => {
                            let header = "Failed to submit retry job: ";
                            let messages = '["';
                            res.errors.forEach(x => {
                                messages += x.message + '",';
                            })

                            messages = messages.slice(0, -1) + ']';

                            createMessageContainer(messages, "error", lqMessageContainer, header);

                            // @ts-ignore
                            model.close("Close model");
                        })
                    } else {
                        createMessageContainer('["Retry job started!"]', "success", lqMessageContainer, "");

                        // @ts-ignore
                        model.accept("Accept run");
                    }
                })
                    .catch(err => {
                        console.log(err);
                        // @ts-ignore
                        model.cancel("Unable to run");
                    });
            }

            var contentContainer = document.createElement("div");
            contentContainer.innerHTML = `<span slot="content" id="modal-description">${generateRunMessage()}</span>`;

            if (overwriteOption === "Append Only") {
                var model = {
                    title: "Retry Errors",
                    theme: "confirmation",
                    contentElement: contentContainer,
                    actions: [
                        {
                            text: "Switch to Append/Overlay",
                            click: function click() {
                                switchToAppendOverlayMode = true;
                                generateModal(switchToAppendOverlayMode)
                            }
                        },
                        {
                            text: "Use " + overwriteOption + " mode again",
                            click: function click() {

                                generateModal(switchToAppendOverlayMode)
                            }
                        },
                        {
                            text: "Cancel",
                            click: function click() {
                                // @ts-ignore
                                model.cancel("Cancel payload");
                            }
                        }
                    ]
                };

                return convenienceApi.modalService.openCustomModal(model);
            } else {
                var model = {
                    title: "Retry Errors",
                    theme: "confirmation",
                    contentElement: contentContainer,
                    actions: [
                        {
                            text: "Ok",
                            click: function click() {
                                generateModal(switchToAppendOverlayMode)
                            }
                        },
                        {
                            text: "Cancel",
                            click: function click() {
                                // @ts-ignore
                                model.cancel("Cancel payload");
                            }
                        }
                    ]
                };

                return convenienceApi.modalService.openCustomModal(model);
            }
        }
    });
}

export function createViewErrorsLink(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, workspaceId: number, integrationPointId: number) {
    return consoleApi.generate.buttonStyledAsLink({
        innerText: "View Errors",
        disabled: !enabled,
        onclick: function () {
            var request = prepareGetViewErrorsPath(convenienceApi, workspaceId, integrationPointId);

            var resp = convenienceApi.relativityHttpClient.get(request.url, request.options)
                .then(function (result) {
                    if (!result.ok) {
                        return ctx.setErrorSummary(["Failed to navigate to view error page."]);
                    } else if (result.ok) {
                        return result.json();
                    }
                });

            resp.then(function (result) {
                var relativityWindow = convenienceApi.utilities.getRelativityPageBaseWindow();
                relativityWindow.location = result[0].value;
            })
        }
    });
}

export function createSaveAsProfileButton(consoleApi, convenienceApi: IConvenienceApi, ctx, workspaceId: number, integrationPointId: number, integrationPoint: object) {
    return consoleApi.generate.button({
        innerText: "Save as Profile",
        onclick: function (e) {
            var contentContainer = document.createElement("div");
            contentContainer.innerHTML = `<rwc-text-input-field id='inputIntegrationPointProfileName' label='Integration point profile name: ' value='${integrationPoint['Name']}' validation-message='You have to provide a propper integration point profile name' required edit-mode></rwc-text-input-field>`;

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
                    return postCreateIntegrationPointProfileRequest(convenienceApi, workspaceId, integrationPointId, name)
                        .then(function (result) {
                            if (!result.ok) {
                                return ctx.setErrorSummary(["Failed to create integration point profile"]);
                            }
                        });
                }
            };

            return convenienceApi.modalService.openCustomModal(model);
        }
    });
}

export function createDownloadErrorFileLink(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, workspaceId: number, integrationPointId: number) {
    return consoleApi.generate.buttonStyledAsLink({
        innerText: "Download Error File",
        disabled: !enabled,
        onclick: function (e) {
            var request = prepareGetImportProviderDocumentAPIRequest(convenienceApi, workspaceId, integrationPointId, "CheckErrorFile");

            return convenienceApi.relativityHttpClient.get(request.url, request.options)
                .then(function (result) {
                    if (!result.ok) {
                        console.log(result);
                        return ctx.setErrorSummary(["The error file could not be found for download. Check Errors tab for details."]);
                    }

                    var request = prepareGetImportProviderDocumentAPIRequest(convenienceApi, workspaceId, integrationPointId, "DownloadErrorFile");
                    window.location.assign(request.url);
                });
        }
    });
}