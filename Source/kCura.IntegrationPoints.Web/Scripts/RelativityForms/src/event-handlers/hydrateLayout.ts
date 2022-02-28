import { contextProvider } from "../helpers/contextProvider";
import { IConvenienceApi } from "../types/convenienceApi";

export function setFieldsValues(layoutData, convenienceApi: IConvenienceApi, sourceConfiguration: Object, destinationConfiguration: Object) {

    var sourceDetails = getSourceDetails(sourceConfiguration, destinationConfiguration["ArtifactTypeName"]);
    var useFolderPathInfo = formatToYesOrNo(convertToBool(destinationConfiguration["UseDynamicFolderPath"]));
    let exportType = getExportType(convertToBool(destinationConfiguration["importNativeFile"]), convertToBool(destinationConfiguration["ImageImport"]));

    convenienceApi.fieldHelper.setValue("Export Type", exportType);
    convenienceApi.fieldHelper.setValue("Source Details", sourceDetails);
    convenienceApi.fieldHelper.setValue("Source Workspace", sourceConfiguration["SourceWorkspace"]);
    convenienceApi.fieldHelper.setValue("Source Rel. Instance", sourceConfiguration["SourceRelativityInstance"]);
    convenienceApi.fieldHelper.setValue("Transferred Objects", destinationConfiguration["ArtifactTypeName"]);
    convenienceApi.fieldHelper.setValue("Destination Workspace", sourceConfiguration["TargetWorkspace"]);
    convenienceApi.fieldHelper.setValue("Destination Folder", sourceConfiguration["TargetFolder"]);
    convenienceApi.fieldHelper.setValue("Overwrite", "");
    convenienceApi.fieldHelper.setValue("Multi-Select Overlay", destinationConfiguration["FieldOverlayBehavior"]);
    convenienceApi.fieldHelper.setValue("Use Folder Path Info", useFolderPathInfo);
    convenienceApi.fieldHelper.setValue("Destination RDO", destinationConfiguration["ArtifactTypeName"]);
    convenienceApi.fieldHelper.setValue("Source Location", sourceConfiguration["LoadFile"]);
    convenienceApi.fieldHelper.setValue("Import Type", getImportType(sourceConfiguration["ImportType"]));
    convenienceApi.fieldHelper.setValue("Destination Details", getDestinationDetails(sourceConfiguration, convenienceApi));
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

    if (sourceConfiguration["IncludeNativeFilesPath"]) {
        console.log("Should not log!");
    }

    convenienceApi.fieldHelper.setValue("File path", getFilePath(sourceConfiguration["FilePath"], sourceConfiguration["IncludeNativeFilesPath"], sourceConfiguration["UserPrefix"]));
    convenienceApi.fieldHelper.setValue("Text and Native File Names", getTextAndNativeFileNames(sourceConfiguration["ExportNativesToFileNamedFrom"], sourceConfiguration["AppendOriginalFileName"], sourceConfiguration["FileNameParts"]));
    convenienceApi.fieldHelper.setValue("Image file format", getImageFileFormat(sourceConfiguration["ExportImages"], sourceConfiguration["SelectedImageDataFileFormat"]));
    convenienceApi.fieldHelper.setValue("Image file type", getImageFileType(sourceConfiguration["ExportImages"], sourceConfiguration["SelectedImageFileType"]));
    convenienceApi.fieldHelper.setValue("Image precedence", getImagePrecedence(sourceConfiguration["ExportImages"], sourceConfiguration["ProductionPrecedence"], sourceConfiguration["ImagePrecedence"]));
    convenienceApi.fieldHelper.setValue("Multiple choice as nested", sourceConfiguration[""]);
    convenienceApi.fieldHelper.setValue("Connection Path", sourceConfiguration["connectionPath"]);
    convenienceApi.fieldHelper.setValue("Object Filter String", sourceConfiguration["filter"]); // "(objectClass=*)"
    convenienceApi.fieldHelper.setValue("Authentication", getConnectionAuthenticationType(sourceConfiguration["connectionAuthenticationType"]));
    convenienceApi.fieldHelper.setValue("Import Nested Items", sourceConfiguration["importNested"]);
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

        console.log("Loadfileformat label: ", fileFormat + "; " + encodingName);
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

function getDestinationDetails(sourceConfiguration: Object, convenienceApi: IConvenienceApi) {
    var destinationLocation;
    if (sourceConfiguration["DestinationLocationId"] > 0) {
        destinationLocation = "Processing Source Location: " + sourceConfiguration["Fileshare"];
    } else {
        destinationLocation = "FileShare: " + ".\\EDDS" + sourceConfiguration["SourceWorkspaceArtifactId"] + "\\" + sourceConfiguration["Fileshare"];
    }

    if (sourceConfiguration["IsAutomaticFolderCreationEnabled"]) {
        convenienceApi.fieldHelper.getValue("Name").then(name => {
            var exportFolderName = "\\" + name + "_{TimeStamp}";
            destinationLocation += exportFolderName;
            return destinationLocation;
        })  
    }

    return destinationLocation;
}

function getSourceDetails(sourceConfiguration, transferredObjects) {
    let sourceDetails;

    if (transferredObjects !== "Document") {
        return "RDO: " + transferredObjects + "; " + sourceConfiguration["ViewName"];
    }
    if (sourceConfiguration["ExportType"] == 3) {
        return "Saved search: " + sourceConfiguration["SavedSearch"];
    }
    if (sourceConfiguration["ExportType"] == 0) {
        return "Folder: " + sourceConfiguration["FolderArtifactName"];
    }
    if (sourceConfiguration["ExportType"] == 1) {
        return "Folder + Subfolders: " + sourceConfiguration["FolderArtifactName"];
    }
    if (sourceConfiguration["ExportType"] == 2) {
        return "Production: " + sourceConfiguration["ProductionName"];
    }

    return sourceDetails;
}

function getExportType(importNatives: boolean, importImages: boolean) {
    let exportType = "Workspace;";
    var images = importImages ? "Images;" : "";
    var natives = importNatives ? "Natives;" : "";
    return exportType + images + natives;
}

function getImportType(importType: number) {
    if (importType === 0) {
        return "Document";
    } else if (importType === 1) {
        return "Images";
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