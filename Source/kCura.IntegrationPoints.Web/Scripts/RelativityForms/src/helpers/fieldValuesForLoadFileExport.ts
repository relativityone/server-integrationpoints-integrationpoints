import { IConvenienceApi } from "../types/convenienceApi";


export async function getDestinationDetails(sourceConfiguration: Object, convenienceApi: IConvenienceApi) {
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

export function getImportType(importType: number) {
    if (importType === 0) {
        return "Document";
    } else if (importType === 1) {
        return "Image";
    } else if (importType === 2) {
        return " Production";
    }
}

export function getVolume(sourceConfiguration: Object) {
    return sourceConfiguration["VolumePrefix"] + "; " + sourceConfiguration["VolumeStartNumber"] + "; " + sourceConfiguration["VolumeDigitPadding"] + "; " + sourceConfiguration["VolumeMaxSize"];
};

export function getSubdirectoryInfo(sourceConfiguration: Object) {
    return (sourceConfiguration["ExportImages"] ? sourceConfiguration["SubdirectoryImagePrefix"] + "; " : "")
        + (sourceConfiguration["ExportNatives"] ? sourceConfiguration["SubdirectoryNativePrefix"] + "; " : "")
        + (sourceConfiguration["ExportFullTextAsFile"] ? sourceConfiguration["SubdirectoryTextPrefix"] + "; " : "")
        + sourceConfiguration["SubdirectoryStartNumber"] + "; " + sourceConfiguration["SubdirectoryDigitPadding"] + "; " + sourceConfiguration["SubdirectoryMaxFiles"];
};

export async function getLoadFileFormat(convenienceApi: IConvenienceApi, workspaceId: number, dataFileEncodingType: string, selectedDataFileFormat: number) {

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

export function getFilePath(filePath: number, includeNativeFilesPath: boolean, userPrefix: string) {
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

export function getImagePrecedence(exportImages: boolean, productionPrecedence, imagePrecedence: Array<any>) {
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

export function getImageFileFormat(exportImages: boolean, selectedImageDataFileFormat: number) {
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

export function getImageFileType(exportImages: boolean, selectedImageFileType: number) {
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

export function getTextAndNativeFileNames(exportNativesToFileNamedFrom: number, appendOriginalFileName: boolean, fileNameParts: Array<string>) {
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

export async function getTextFileEncoding(convenienceApi: IConvenienceApi, workspaceId: number, dataFileEncodingName: string) {
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

export function getPrecedenceList(sourceConfiguration: Object) {
    var text = "";
    if (sourceConfiguration["ExportFullTextAsFile"] && sourceConfiguration["TextPrecedenceFields"].length > 0) {
        for (var i = 0; i < sourceConfiguration["TextPrecedenceFields"].length; i++) {
            text += sourceConfiguration["TextPrecedenceFields"][i].displayName + "; ";
        }
        text = text.substring(0, text.length - 2);
    }
    return text;
}