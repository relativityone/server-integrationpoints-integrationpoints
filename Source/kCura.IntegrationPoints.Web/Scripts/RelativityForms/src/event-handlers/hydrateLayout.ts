import { IConvenienceApi } from "../types/convenienceApi";

export function setFieldsValues(layoutData, convenienceApi: IConvenienceApi, sourceConfiguration: Object, destinationConfiguration: Object) {

    var sourceDetails = getSourceDetails(sourceConfiguration, destinationConfiguration["ArtifactTypeName"]);
    var useFolderPathInfo = formatToYesOrNo(convertToBool(destinationConfiguration["UseDynamicFolderPath"]));
    let exportType = getExportType(sourceConfiguration, destinationConfiguration);

    // export 
    convenienceApi.fieldHelper.setValue("Export Type", exportType);
    convenienceApi.fieldHelper.setValue("Source Details", sourceDetails);

    // export to relativity
    convenienceApi.fieldHelper.setValue("Source Workspace", sourceConfiguration["SourceWorkspace"]);
    convenienceApi.fieldHelper.setValue("Source Rel. Instance", sourceConfiguration["SourceRelativityInstance"]);
    convenienceApi.fieldHelper.setValue("Transfered Object", destinationConfiguration["ArtifactTypeName"]);
    convenienceApi.fieldHelper.setValue("Destination Workspace", sourceConfiguration["TargetWorkspace"]);
    convenienceApi.fieldHelper.setValue("Destination Folder", sourceConfiguration["TargetFolder"]);
    convenienceApi.fieldHelper.setValue("Destination Production Set", sourceConfiguration["targetProductionSet"]);
    convenienceApi.fieldHelper.setValue("Multi-Select Overlay", destinationConfiguration["FieldOverlayBehavior"]);
    convenienceApi.fieldHelper.setValue("Use Folder Path Info", useFolderPathInfo);
    convenienceApi.fieldHelper.setValue("Move Existing Docs", formatToYesOrNo(destinationConfiguration["MoveExistingDocuments"]));
    convenienceApi.fieldHelper.setValue("Image Precedence", getPrecenenceSummary(destinationConfiguration));
    convenienceApi.fieldHelper.setValue("Copy Files to Repository", formatToYesOrNo(destinationConfiguration["importNativeFile"]));

    // import from load file
    convenienceApi.fieldHelper.setValue("Destination RDO", destinationConfiguration["ArtifactTypeName"]);
    convenienceApi.fieldHelper.setValue("Source Location", sourceConfiguration["LoadFile"]);
    convenienceApi.fieldHelper.setValue("Import Type", getImportType(sourceConfiguration["ImportType"]));

    // export to load file 
    getDestinationDetails(sourceConfiguration, convenienceApi).then(label => {
        convenienceApi.fieldHelper.setValue("Destination Details", label);
    })

    convenienceApi.fieldHelper.setValue("Overwrite Files", formatToYesOrNo(destinationConfiguration["OverwriteFiles"]));
    convenienceApi.fieldHelper.setValue("Start at record", sourceConfiguration["StartExportAtRecord"]);
    convenienceApi.fieldHelper.setValue("Volume", getVolume(sourceConfiguration));
    convenienceApi.fieldHelper.setValue("Subdirectory", getSubdirectoryInfo(sourceConfiguration));
    getLoadFileFormat(
        convenienceApi,
        sourceConfiguration["SourceWorkspaceArtifactId"],
        sourceConfiguration["DataFileEncodingType"],
        sourceConfiguration["SelectedDataFileFormat"]).then(label => {
            console.log("Load File Format: ", label);
            convenienceApi.fieldHelper.setValue("Load file format", label);
        })

    convenienceApi.fieldHelper.setValue("File path", getFilePath(sourceConfiguration["FilePath"], sourceConfiguration["IncludeNativeFilesPath"], sourceConfiguration["UserPrefix"]));
    convenienceApi.fieldHelper.setValue("Text and Native File Names", getTextAndNativeFileNames(sourceConfiguration["ExportNativesToFileNamedFrom"], sourceConfiguration["AppendOriginalFileName"], sourceConfiguration["FileNameParts"]));
    convenienceApi.fieldHelper.setValue("Image file format", getImageFileFormat(sourceConfiguration["ExportImages"], sourceConfiguration["SelectedImageDataFileFormat"]));
    convenienceApi.fieldHelper.setValue("Image file type", getImageFileType(sourceConfiguration["ExportImages"], sourceConfiguration["SelectedImageFileType"]));
    convenienceApi.fieldHelper.setValue("Image precedence", getImagePrecedence(sourceConfiguration["ExportImages"], sourceConfiguration["ProductionPrecedence"], sourceConfiguration["ImagePrecedence"]));
    convenienceApi.fieldHelper.setValue("Text precedence", getPrecedenceList(sourceConfiguration));
    getTextFileEncoding(
        convenienceApi,
        sourceConfiguration["SourceWorkspaceArtifactId"],
        sourceConfiguration["TextFileEncodingType"]
    ).then(label => {
        convenienceApi.fieldHelper.setValue("Text file encoding", label);
    })
    convenienceApi.fieldHelper.setValue("Multiple choice as nested", (sourceConfiguration["ExportMultipleChoiceFieldsAsNested"] ? "Yes" : "No"));

    // import from LDAP
    convenienceApi.fieldHelper.setValue("Connection Path", sourceConfiguration["connectionPath"]);
    convenienceApi.fieldHelper.setValue("Object Filter String", getFilter(sourceConfiguration["filter"]));
    convenienceApi.fieldHelper.setValue("Authentication", getConnectionAuthenticationType(sourceConfiguration["connectionAuthenticationType"]));
    convenienceApi.fieldHelper.setValue("Import Nested Items", formatToYesOrNo(convertToBool(sourceConfiguration["importNested"])));

    // import from FTP
    convenienceApi.fieldHelper.setValue("Host", sourceConfiguration["Host"]);
    convenienceApi.fieldHelper.setValue("Port", sourceConfiguration["Port"]);
    convenienceApi.fieldHelper.setValue("Protocol", sourceConfiguration["Protocol"]);
    convenienceApi.fieldHelper.setValue("Filename Prefix", sourceConfiguration["FileNamePrefix"]);
    convenienceApi.fieldHelper.setValue("Timezone Offset", sourceConfiguration["TimezoneOffset"]);
}

async function getTextFileEncoding(convenienceApi: IConvenienceApi, workspaceId: number, dataFileEncodingName: string) {
    var request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/GetAvailableEncodings"
    }

    var resp = convenienceApi.relativityHttpClient.get(request.url, request.options)
        .then(function (result) {
            if (!result.ok) {
                console.log("error in get; ", result);
            } else if (result.ok) {
                return result.json();
            }
        });

    let label = await resp.then(function (result: Array<Object>) {
        let encodingName;
        result.forEach(el => {
            if (el["name"] === dataFileEncodingName) {
                encodingName = el["displayName"];
            }
        })
        return encodingName;
    })

    return label;
}

function getPrecedenceList(sourceConfiguration: Object) {
    var text = "";
    if (sourceConfiguration["ExportFullTextAsFile"] && sourceConfiguration["TextPrecedenceFields"].length > 0) {
        for (var i = 0; i < sourceConfiguration["TextPrecedenceFields"].length; i++) {
            text += sourceConfiguration["TextPrecedenceFields"][i].displayName + "; ";
        }
        text = text.substring(0, text.length - 2);
    }
    return text;
}

function getPrecenenceSummary(destinationConfiguration: Object) {
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

function getFilter(filter) {
    if (filter) {
        return filter;
    } else {
        return "(objectClass=*)";
    }
}

function getConnectionAuthenticationType(connectionAuthenticationType: number) {
    if (connectionAuthenticationType === 16) {
        return "Anonymous"
    } else if (connectionAuthenticationType === 32) {
        return "FastBind"
    } else if (connectionAuthenticationType === 2) {
        return "Secure Socket Layer"
    }
    return "";
}

function getImagePrecedence(exportImages: boolean, productionPrecedence, imagePrecedence: Array<any>) {
    var text = "";
    if (exportImages) {
        if (productionPrecedence === 0) {
            text = "Original";
        }
        else {
            text = "Produced: ";
            if (imagePrecedence.length > 0) {
                for (var i = 0; i < imagePrecedence.length; i++) {
                    text += imagePrecedence[i].displayName + "; ";
                }
            }
        }
    }
    return text;
};

function getImageFileFormat(exportImages: boolean, selectedImageDataFileFormat: number) {
    if (exportImages) {
        if (selectedImageDataFileFormat === 0) {
            return "Opticon";
        } else if (selectedImageDataFileFormat === 1) {
            return "IPRO"
        } else if (selectedImageDataFileFormat === 2) {
            return "IPRO (FullText)"
        } else if (selectedImageDataFileFormat === 3) {
            return "No Image Load File"
        }
    }
    return ""
}

function getImageFileType(exportImages: boolean, selectedImageFileType: number) {
    if (exportImages) {
        if (selectedImageFileType === 0) {
            return "Single page TIFF/JPEG";
        } else if (selectedImageFileType === 1) {
            return "Multi page TIFF/JPEG"
        } else if (selectedImageFileType === 2) {
            return "PDF"
        } 
    }
    return ""
}

function getTextAndNativeFileNames(exportNativesToFileNamedFrom: number, appendOriginalFileName: boolean, fileNameParts: Array<string>) {
    var exportNamingType;
    if (exportNativesToFileNamedFrom === 0) {
        exportNamingType = "Identifier";
    } else if (exportNativesToFileNamedFrom === 1) {
        exportNamingType = "Begin production number";
    } else if (exportNativesToFileNamedFrom === 2) {
        exportNamingType = "Custom";
    }

    //if naming after 'Identifier' or afer 'Begin production number'
    if (exportNativesToFileNamedFrom === 0 || exportNativesToFileNamedFrom === 1) {
        return "Named after: " + exportNamingType + (appendOriginalFileName ? "; Append Original File Names" : "");
    }
    //if using custom naming pattern
    else if (exportNativesToFileNamedFrom === 2) {
        var result = exportNamingType + ": ";
        fileNameParts.forEach(function (part) {
           if (part["type"] === 'F') {
                result += "{" + part["name"] + "}";
           }
           else if (part["type"] === 'S') {
                result += part["value"];
           }
        });
        return result + ".{File Extension}";
    }
};

function getFilePath(filePath: number, includeNativeFilesPath: boolean, userPrefix: string) {
    var filePathType = "";
    switch (filePath) {
        case (0):
            filePathType = "Relative";
        case (1):
            filePathType = "Absolute";
        case (2):
            filePathType = "User Prefix";
    }

    return (includeNativeFilesPath ? "Include" : "Do not include")
        + ("; " + filePathType)
        + (filePath == 2 ? (": " + userPrefix) : "");
}

async function getLoadFileFormat(convenienceApi: IConvenienceApi, workspaceId: number, dataFileEncodingType: string, selectedDataFileFormat: number) {

    var request = {
        options: convenienceApi.relativityHttpClient.makeRelativityBaseRequestOptions({
            headers: {
                "content-type": "application/json; charset=utf-8"
            }
        }),
        url: convenienceApi.applicationPaths.relativity + "CustomPages/DCF6E9D1-22B6-4DA3-98F6-41381E93C30C/" + workspaceId + "/api/GetAvailableEncodings"
    }

    var resp = convenienceApi.relativityHttpClient.get(request.url, request.options)
        .then(function (result) {
            if (!result.ok) {
                console.log("error in get; ", result);
            } else if (result.ok) {
                return result.json();
            }
        });

    let label = await resp.then(function (result: Array<Object>) {
        let encodingName;
        result.forEach(el => {
            if (el["name"] === dataFileEncodingType) {
                encodingName = el["displayName"];
            }
        })

        let fileFormat = "";
        switch (selectedDataFileFormat) {
            case (0):
                fileFormat = "Relativity (.dat)";
                break;
            case (1):
                fileFormat = "HTML (.html)";
                break;
            case (2):
                fileFormat = "Comma-separated (.csv)";
                break;
            case (3):
                fileFormat = "Custom (.txt)";
                break;
        }
        return fileFormat + "; " + encodingName;
    })

    return label;
}

function getVolume(sourceConfiguration: Object) {
    return sourceConfiguration["VolumePrefix"] + "; " + sourceConfiguration["VolumeStartNumber"] + "; " + sourceConfiguration["VolumeDigitPadding"] + "; " + sourceConfiguration["VolumeMaxSize"];
};

function getSubdirectoryInfo(sourceConfiguration: Object) {
    return (sourceConfiguration["ExportImages"] ? sourceConfiguration["SubdirectoryImagePrefix"] + "; " : "")
        + (sourceConfiguration["ExportNatives"] ? sourceConfiguration["SubdirectoryNativePrefix"] + "; " : "")
        + (sourceConfiguration["ExportFullTextAsFile"] ? sourceConfiguration["SubdirectoryTextPrefix"] + "; " : "")
        + sourceConfiguration["SubdirectoryStartNumber"] + "; " + sourceConfiguration["SubdirectoryDigitPadding"] + "; " + sourceConfiguration["SubdirectoryMaxFiles"];
};

async function getDestinationDetails(sourceConfiguration: Object, convenienceApi: IConvenienceApi) {
    var destinationLocation;
    if (sourceConfiguration["DestinationLocationId"] > 0) {
        destinationLocation = "Processing Source Location: " + sourceConfiguration["Fileshare"];
    } else {
        destinationLocation = "FileShare:" + ".\\EDDS" + sourceConfiguration["SourceWorkspaceArtifactId"] + "\\" + sourceConfiguration["Fileshare"];
    }

    let label = destinationLocation;

    if (sourceConfiguration["IsAutomaticFolderCreationEnabled"]) {
        label = await convenienceApi.fieldHelper.getValue("Name").then(name => {
            var exportFolderName = "\\" + name + "_{TimeStamp}";
            destinationLocation += exportFolderName;
            return destinationLocation;
        })
    } 

    return label;
}

function getSourceDetails(sourceConfiguration, transferredObjects) {

    if (sourceConfiguration["SourceProductionId"]) {
        return "Production Set: " + sourceConfiguration["SourceProductionName"];
    } else if (sourceConfiguration["SavedSearchArtifactId"]){
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

function getExportType(sourceConfiguration: Object, destinationConfiguration: Object) {
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

function getImportType(importType: number) {
    if (importType === 0) {
        return "Document";
    } else if (importType === 1) {
        return "Image";
    } else if(importType ===2) {
        return " Production";
    }
}

function convertToBool(value) {
    return value === "true";
}

function formatToYesOrNo(value) {
    return convertToBool(value) ? "Yes" : "No";
};