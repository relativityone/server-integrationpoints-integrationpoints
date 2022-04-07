import { IConvenienceApi } from "../types/convenienceApi";
import { postCreateIntegrationPointProfileRequest, postJobAPIRequest, prepareGetImportProviderDocumentAPIRequest, prepareGetViewErrorsPath } from "./buttonFunctionalities";

export function createRunButton(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, workspaceId: number, integrationPointId: number) {
    return consoleApi.generate.button({
        innerText: "Run",
        disabled: !enabled,
        onclick: function () {
            function generateRunMessage() {
                return "Are you sure you want to run this job now?";
            }

            return convenienceApi.modalService.confirm({
                title: "Run",
                message: generateRunMessage(),
                acceptText: "Ok",
                cancelText: "Cancel",
                acceptAction: function () {
                    return postJobAPIRequest(convenienceApi, workspaceId, integrationPointId)
                        .then(function (result) {
                            if (!result.ok) {
                                console.log(result);
                                return ctx.setErrorSummary(["Failed to submit integration job. Check Errors tab for details."]);
                            }
                        });
                }
            });
        }
    });
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

export function createRetryErrorsButton(consoleApi, convenienceApi: IConvenienceApi, ctx, enabled: boolean, workspaceId: number, integrationPointId: number, overwriteOption: string) {

    return consoleApi.generate.button({
        innerText: "Retry Errors",
        disabled: !enabled,
        onclick: function () {
            function generateRunMessage() {

                var selectedMessage = "";
                if (overwriteOption === "Overlay Only") {
                    selectedMessage = "The retry job will run in Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
                } else {
                    selectedMessage = "The retry job will run in Append/Overlay mode. Document metadata with the same identifier will be overwritten in the target workspace. Would you still like to proceed?";
                }
                return selectedMessage;
            }

            return convenienceApi.modalService.confirm({
                title: "Retry Errors",
                message: generateRunMessage(),
                acceptText: "Ok",
                cancelText: "Cancel",
                acceptAction: function () {
                    return postJobAPIRequest(convenienceApi, workspaceId, integrationPointId, "Retry")
                        .then(function (result) {
                            if (!result.ok) {
                                console.log(result);
                                return ctx.setErrorSummary(["Failed to submit retry job. Check Errors tab for details."]);
                            }
                        });
                }
            });
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
        innerText: "Save as a Profile",
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