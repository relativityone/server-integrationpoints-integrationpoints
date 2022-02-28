import { IConvenienceApi } from "../types/convenienceApi";

export function transformLayout(layoutData, convenienceApi: IConvenienceApi, backingModelData) {
    try {
        let [sourceConfiguration, destinationConfiguration, sourceProvider] = extractFieldsValuesFromBackingModelData(backingModelData);
        console.log("in transform: ", sourceConfiguration, destinationConfiguration, sourceProvider);

        var existingFields = convenienceApi.layout.getFields(layoutData);
        var fieldsForRelativityExport = ["Export Type", "Source Details", "Source Workspace", "Source Rel. Instance", "Transferred Object", "Destination Workspace", "Destination Folder", "Owerwrite", "Multi-Select Overlay", "Use Folder Path Info"];
        var fieldsForLoadFile = ["Destination RDO", "Source Location", "Import Type"];
        var fieldsForLoadFileExport = ["Export Type", "Source Details", "Destination Details", "Overwrite Files", "Start at record", "Volume", "Subdirectory", "Load file format", "File path", "Text and Native File Names", "Image file format", "Image file type", "Image precedence", "Multiple choice as nested"];
        var fieldsForLDAP = ["Connection Path", "Object Filter String", "Authentication", "Import Nested Items"];
        var fieldsForFTP = [];

        //fields to display depend on source provider type 
        switch (sourceProvider) {
            case "Relativity":
                if (destinationConfiguration["Provider"] === "relativity") {
                    addFieldsToLayout(layoutData, existingFields, fieldsForRelativityExport)
                } else if (destinationConfiguration["Provider"] === "Load File") {
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
    let pos = 7;
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