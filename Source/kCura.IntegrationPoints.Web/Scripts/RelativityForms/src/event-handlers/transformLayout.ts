import { IConvenienceApi } from "../types/convenienceApi";

export function transformLayout(layoutData, convenienceApi: IConvenienceApi, backingModelData) {
    try {
        let [sourceConfiguration, destinationConfiguration, sourceProvider] = extractFieldsValuesFromBackingModelData(backingModelData);

        var existingFields = convenienceApi.layout.getFields(layoutData);

        var fieldsForLoadFile = ["Destination RDO", "Source Location", "Import Type"];
        var fieldsForLDAP = ["Connection Path", "Object Filter String", "Authentication", "Import Nested Items"];
        var fieldsForFTP = ["Destination RDO", "Host", "Port", "Protocol", "Filename Prefix", "Timezone Offset"];

        //fields to display depend on source provider type 
        switch (sourceProvider) {
            case "Relativity":
                if (destinationConfiguration["Provider"] === "relativity") {
                    let fieldsForRelativityExport = prepareFieldsForRelativityExport(sourceConfiguration, destinationConfiguration)
                    addFieldsToLayout(layoutData, existingFields, fieldsForRelativityExport)
                    if (destinationConfiguration["artifactTypeID"] == 10 && sourceConfiguration["SourceViewId"] != 10) {
                        let fieldsForRelativityExportSecondColumn = prepareFieldsForRelativityExportSecondColumn(destinationConfiguration);
                        addFieldsToLayoutSecondColumn(layoutData, fieldsForRelativityExportSecondColumn);
                    }
                } else if (destinationConfiguration["Provider"] === "Load File") {
                    let fieldsForLoadFileExport = prepareFieldsForLoadFileExport(sourceConfiguration, destinationConfiguration);
                    addFieldsToLayout(layoutData, existingFields, fieldsForLoadFileExport)
                } 
                break;
            case "Load File":
                addFieldsToLayout(layoutData, existingFields, fieldsForLoadFile)
                break;
            case "LDAP":
                addFieldsToLayout(layoutData, existingFields, fieldsForLDAP)
                break;
            case "FTP (CSV File)":
                addFieldsToLayout(layoutData, existingFields, fieldsForFTP)
                break;
            default:
                console.log("other case?");
        }

        return [sourceConfiguration, destinationConfiguration];
    } catch (err) {
        console.log(err);
    }
};

function extractFieldsValuesFromBackingModelData(backingModelData: Object) {
    let keys = Object.keys(backingModelData);
    keys.sort((a, b) => { return Number(a) - Number(b) });

    let sourceConfiguration = JSON.parse(backingModelData[keys[4].toString()]);
    let destinationConfiguration = JSON.parse(backingModelData[keys[5].toString()]);
    let sourceProvider = backingModelData[keys[6].toString()].Name;
    return [sourceConfiguration, destinationConfiguration, sourceProvider];
}

function addFieldsToLayout(layoutData, existingFields, fields: Array<String>) {
    let pos = 6;

    existingFields[3].Row = 2;
    existingFields[5].Row = 3;
    existingFields[7].Row = 4;
    existingFields[8].Row = 5;

    fields.forEach((label, index) => {

        //first two fields overwrite existing source and destination configuration - they are no. 7 and 8 in layout fields array 
        if (index < 2) {
            existingFields[7 + index].DisplayName = label;
        } else {
            // the rest of new fields are added below - starting at row 7 
            layoutData[0].Elements[0].Elements.push({
                FieldType: "FixedLengthText",
                MaxLength: 255,
                AllowHTML: false,
                FieldID: label,
                DisplayName: label,
                AllowCopyFromPrevious: false,
                ShowNameColumn: true,
                IsReadOnly: false,
                IsRequired: false,
                IsSystem: false,
                FieldCategoryID: 2,
                EnableDataGrid: false,
                Guids: [
                    "b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a"
                ],
                Colspan: 1,
                Column: 1,
                Row: pos,
                expanded: false,
                minimumWidth: -1,
                IsVisible: true
            });
            pos += 1;
        }
    })
}

function addFieldsToLayoutSecondColumn(layoutData, fields: Array<String>) {
    let pos = 6;
    fields.forEach((label, index) => {

        // the rest of new fields are added below - starting at row 7 
        layoutData[0].Elements[0].Elements.push({
            FieldType: "FixedLengthText",
            MaxLength: 255,
            AllowHTML: false,
            FieldID: label,
            DisplayName: label,
            AllowCopyFromPrevious: false,
            ShowNameColumn: true,
            IsReadOnly: false,
            IsRequired: false,
            IsSystem: false,
            FieldCategoryID: 2,
            EnableDataGrid: false,
            Guids: [
                "b1323ca7-34e5-4e6b-8ff1-e8d3b1a5fd0a"
            ],
            Colspan: 1,
            Column: 2,
            Row: pos,
            expanded: false,
            minimumWidth: -1,
            IsVisible: true
        });
        pos += 1;
        })
}

function prepareFieldsForLoadFileExport(sourceConfiguration: Object, destinationConfiguration: Object) {
    var fieldsForLoadFileExport = ["Export Type", "Source Details", "Destination Details", "Overwrite Files", "Start at record"]; 
    if (sourceConfiguration["ExportNatives"] || sourceConfiguration["ExportImages"] || sourceConfiguration["ExportFullTextAsFile"]) {
        fieldsForLoadFileExport.push("Volume", "Subdirectory");
    }
    fieldsForLoadFileExport.push("Load file format", "File path", "Text and Native File Names");
    if (sourceConfiguration["ExportImages"]) {
        fieldsForLoadFileExport.push("Image file format", "Image file type");
        if (sourceConfiguration["ExportType"] !== 2) {
            fieldsForLoadFileExport.push("Image precedence");
        }
    }
    if (sourceConfiguration["ExportFullTextAsFile"]) {
        fieldsForLoadFileExport.push("Text precedence", "Text file encoding");
    }
    fieldsForLoadFileExport.push("Multiple choice as nested");
    return fieldsForLoadFileExport;
}

function prepareFieldsForRelativityExport(sourceConfiguration: Object, destinationConfiguration: Object) {

    var fieldsForRelativityExport = ["Export Type", "Source Details", "Source Workspace"];

    if (destinationConfiguration["FederatedInstanceArtifactId"] !== null) {
        fieldsForRelativityExport.push("Source Rel. Instance");
    }

    fieldsForRelativityExport.push("Transfered Object", "Destination Workspace");

    if (sourceConfiguration["TargetFolder"] !== undefined) {
        fieldsForRelativityExport.push("Destination Folder");
    }

    fieldsForRelativityExport.push("Multi-Select Overlay");

    if (destinationConfiguration["ArtifactTypeName"] === "Document"){

        if (sourceConfiguration["targetProductionSet"] !== undefined) {
            fieldsForRelativityExport.push("Destination Production Set");
        }

        fieldsForRelativityExport.push("Use Folder Path Info");

        if (destinationConfiguration["ImageImport"] !== "true") {
            fieldsForRelativityExport.push("Move Existing Docs");
        } else {
            fieldsForRelativityExport.push("Image Precedence", "Copy Files to Repository");
        }    
    }

    return fieldsForRelativityExport;
}

function prepareFieldsForRelativityExportSecondColumn(destinationConfiguration: Object) {
    var fieldsForRelativityExportSecondColumn = ["Total of Documents"];

    if (destinationConfiguration["importNativeFile"] == 'true' && !importImageFiles(destinationConfiguration)) {
        fieldsForRelativityExportSecondColumn.push("Total of Natives");
    }

    if (importImageFiles(destinationConfiguration)) {
        fieldsForRelativityExportSecondColumn.push("Total of Images");
    }

    fieldsForRelativityExportSecondColumn.push("Create Saved Search");

    return fieldsForRelativityExportSecondColumn;
}

function importImageFiles(destinationConfiguration: Object) {
    return (destinationConfiguration["ImageImport"] == 'true' && (!destinationConfiguration["ImagePrecedence"] || destinationConfiguration["ImagePrecedence"].length == 0));
}