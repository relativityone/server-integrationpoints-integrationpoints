import { contextProvider } from "../helpers/contextProvider";
import { IConvenienceApi } from "../types/convenienceApi";
import { ButtonState } from "../types/buttonState";
import { createDownloadErrorFileLink, createRetryErrorsButton, createRunButton, createSaveAsProfileButton, createStopButton, createViewErrorsLink, removeMessageContainers } from "../helpers/buttonCreate";

export function createConsole(convenienceApi: IConvenienceApi): void {
    return contextProvider((ctx) => {
        var consoleApi = convenienceApi.console;
        var integrationPointId = ctx.artifactId;
        var workspaceId = ctx.workspaceId;

        var buttonState = getButtonStateObject(convenienceApi, ctx, workspaceId, integrationPointId);
        buttonState.then(function (btnStateObj: ButtonState) {
            var consoleContent = generateConsoleContent(convenienceApi, ctx, workspaceId, integrationPointId, btnStateObj);

            return consoleApi.destroy().then(function () {
                return consoleApi.containersPromise;
            }).then(function (containers) {
                containers.rootElement.appendChild(consoleContent);
                let relativityWindow = convenienceApi.utilities.getRelativityPageBaseWindow();
                checkIfRefreshIsNeeded(btnStateObj, convenienceApi, ctx, workspaceId, integrationPointId, relativityWindow.location.href);
            });
        })
    })
}

function checkIfRefreshIsNeeded(btnStateObj, convenienceApi, ctx, workspaceId, integrationPointId, currentPage): void {
    var buttonState = getButtonStateObject(convenienceApi, ctx, workspaceId, integrationPointId);
    let relativityWindow = convenienceApi.utilities.getRelativityPageBaseWindow();
    buttonState.then(function (newBtnStateObj: ButtonState) {
        if (typeof newBtnStateObj === 'undefined') {
            throw new TypeError("Button state is undefined");
        }
        else if(compareButtonStates(btnStateObj, newBtnStateObj)) {
            if (currentPage === relativityWindow.location.href) {
                setTimeout(checkIfRefreshIsNeeded, 5000, newBtnStateObj, convenienceApi, ctx, workspaceId, integrationPointId, relativityWindow.location.href);
            }
        } else {
            var consoleContent = generateConsoleContent(convenienceApi, ctx, workspaceId, integrationPointId, newBtnStateObj);

            return convenienceApi.console.destroy().then(function () {
                return convenienceApi.console.containersPromise;
            }).then(function (containers) {
                containers.rootElement.appendChild(consoleContent);
                if (currentPage === relativityWindow.location.href) {
                    setTimeout(checkIfRefreshIsNeeded, 5000, newBtnStateObj, convenienceApi, ctx, workspaceId, integrationPointId, relativityWindow.location.href);
                }
            });
        }
    }).catch(err => {
        console.log("Error occured while getting button state, will try once again in 5 secs", err)
        if (currentPage === relativityWindow.location.href) {
            setTimeout(checkIfRefreshIsNeeded, 5000, btnStateObj, convenienceApi, ctx, workspaceId, integrationPointId, relativityWindow.location.href);
        }
    })
}

function compareButtonStates(previousState: ButtonState, newState: ButtonState): boolean {
    for (const [key, value] of Object.entries(previousState)) {
        if (value != newState[key]) {
            return false;
        }
    }
    return true;
}

async function getButtonStateObject(convenienceApi, ctx, workspaceId, integrationPointId) {
    var request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/ConsoleState/GetConsoleState" + '?workspaceId=' + workspaceId + '&integrationPointArtifactId=' + integrationPointId
    };

    var resp = await convenienceApi.relativityHttpClient.get(request.url, request.options)
        .then(function (result) {
            if (!result.ok) {
                return ctx.setErrorSummary(["Unable to generate button console - failed to get button state."]);
            } else if (result.ok) {
                return result.json();
            }
        });
    return resp;
}

function generateConsoleContent(convenienceApi, ctx, workspaceId, integrationPointId, btnStateObj) {

    var buttonState: ButtonState = btnStateObj

    var consoleApi = convenienceApi.console;

    var integrationPoint = {};

    ctx.fieldNameToFieldIdMap.forEach(function (value, key) {
        var trimmedKey = key.toString().replace(/\s/g, '');
        try {
            var val = JSON.parse(ctx.backingModelData[value]);
        } catch (e) {
            var val = ctx.backingModelData[value];
        }
        integrationPoint[trimmedKey] = val;
    });

    var transferOptionsTitle = consoleApi.generate.sectionTitle({
        innerText: "Transfer Options",
        id: "transferOptionsUniqueId"
    });

    var transferSection = [transferOptionsTitle];

    if (buttonState.runButtonEnabled) {
        var runButton = createRunButton(consoleApi, convenienceApi, ctx, buttonState.runButtonEnabled, workspaceId, integrationPointId, document.getElementsByTagName("lq-message-container")[0]);
        transferSection.push(runButton);
    }

    if (buttonState.stopButtonEnabled) {
        var stopButton = createStopButton(consoleApi, convenienceApi, ctx, buttonState.stopButtonEnabled, workspaceId, integrationPointId);
        transferSection.push(stopButton);
    }

    if (buttonState.retryErrorsButtonVisible && integrationPoint["ExportType"].artifactTypeID === 10) {
        var retryErrorsButton = createRetryErrorsButton(consoleApi, convenienceApi, ctx, buttonState.retryErrorsButtonEnabled, workspaceId, integrationPointId, integrationPoint["OverwriteFields"].Name);
        transferSection.push(retryErrorsButton);
    }

    if (buttonState.viewErrorsLinkVisible) {
        var viewErrorsLink = createViewErrorsLink(consoleApi, convenienceApi, ctx, buttonState.viewErrorsLinkEnabled, workspaceId, integrationPointId);
        transferSection.push(viewErrorsLink);
    }

    if (buttonState.saveAsProfileButtonVisible) {
        var saveAsProfileButton = createSaveAsProfileButton(consoleApi, convenienceApi, ctx, workspaceId, integrationPointId, integrationPoint);
        transferSection.push(saveAsProfileButton);
    }

    if (buttonState.downloadErrorFileLinkVisible) {
        var downloadErrorFileLink = createDownloadErrorFileLink(consoleApi, convenienceApi, ctx, buttonState.downloadErrorFileLinkEnabled, workspaceId, integrationPointId);
        transferSection.push(downloadErrorFileLink);
    }

    var transferOptionsSection = consoleApi.generate.section({}, transferSection);

    return transferOptionsSection;
}