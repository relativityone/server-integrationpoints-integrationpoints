import { IConvenienceApi } from "../types/convenienceApi";

export function transformLayout(layoutData, convenienceApi: IConvenienceApi) {

    layoutData[0].Elements[0].Elements.splice(2, 0, {
        FieldType: "FixedLengthText",
        MaxLength: 255,
        AllowHTML: false,
        FieldID: "Export Type",
        DisplayName: "Export Type",
        AllowCopyFromPrevious: false,
        ShowNameColumn: true,
        IsReadOnly: false,
        IsRequired: false,
        IsSystem: false,
        FieldCategoryID: 2,
        EnableDataGrid: false,
        Guids: [
            "d534f433-dd92-4a53-b12d-bf85472e6d7a"
        ],
        Colspan: 1,
        Column: 1,
        Row: 2,
        expanded: false,
        minimumWidth: -1,
        IsVisible: true
    });

    let fieldsToAdd = [
        "Source Rel. Instance",
        "Transferred Objects",
        "Destination Workspace",
        "Destination Folder",
        "Multi-Select Overlay",
        "Use Folder Path Info",
        "Move Existing Docs"
    ];

    let position = 7;
    fieldsToAdd.forEach(label => {
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
            Row: position,
            expanded: false,
            minimumWidth: -1,
            IsVisible: true
        });
        position += 1;
    })

    position = 6;
    ["Total of Documents", "Create Saved Search"].forEach(label => {
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
            Row: position,
            expanded: false,
            minimumWidth: -1,
            IsVisible: true
        });
        position += 1;
    })

    var fields = convenienceApi.layout.getFields(layoutData);

    fields[8].DisplayName = "Source Details";
    fields[9].DisplayName = "Source Workspace";
};