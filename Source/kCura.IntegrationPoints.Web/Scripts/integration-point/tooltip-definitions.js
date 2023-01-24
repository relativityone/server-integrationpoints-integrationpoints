var TooltipDefs = {}

TooltipDefs.ExportDetails = [
    {
        name: "Export Type",
        description: "this field reflects the type of export job you're running, which you specified on the Setup layout. " +
                "By default, Load File is selected as the base configuration for your export job. " +
                "You also have the option of enhancing the load file through by selecting Images, Natives, " +
                "and Text Fields as Files, depending on the makeup of your data. " +
                "Selecting any of these makes additional corresponding output settings available.",
        subsection: []
    },
    {
        name: "Destination Folder",
        description: "specify the folder into which you want to export the data from the load file. " +
            "This reads the Default File Repository defined for the Workspace and allows you to select a subfolder of that location. " +
            "Contact Sys Admin for more details.",
        subsection: []
    },
    {
        name: "Create Export Folder",
        description: "when this option is selected an unique export folder will be created with a name of integration point job and a run timestamp.",
        subsection: []
    }
];

TooltipDefs.ExportDetailsTitle = "Export Details:";

TooltipDefs.RelativityProviderDestinationDetails = [
    {
        name: "Workspace",
        description: "the workspace to which you want to promote the data from your source.",
        subsection: []
    },
    {
        name: "Folder",
        description: "the top/parent folder inside the destination workspace into which you want to export the files.",
        subsection: []
    },
    {
        name: "Production Set",
        description: "the production set in the destination workspace to which you want to transfer images. " +
            "Only production sets that have not yet been run are available for selection here. " +
            "Use this field in conjunction with the Copy Images, Image Precedence, " +
            "and Production Precedence fields on the Map Fields layout to determine how the images are transferred.",
        subsection: []
    },
    {
        name: "Plus (+) button",
        description: "by clicking this button you may create a new Production Set in the destination workspace and provide a name for created Production Set, " +
            "other values will be defaulted.",
        subsection: []
    },
    {
        name: "Create Saved Search",
        description: "when Create Saved Search is set to Yes it will create a Save Search in a destination folder with a condition to easily locate copied data with this job. " +
            "Please note that Save Search name is limited to 50 characters and will be cut if exceeded.",
        subsection: []
    },
    {
        name: "Enable Tagging",
        description: "Documents tagging allows to easily track transferred documents in a destination workspace, via Relativity Source Case and Relativity Source Job fields." +
            "However the tagging operation is time and resources consuming, therefore it is advised to turn it off for better Integration Points performance. " +
            "Please note that Create Saved Search option is based on documents tagging capability, therefore selection that option enables the tagging by default.",
        subsection: []
    }
];

TooltipDefs.RelativityProviderDestinationDetailsTitle = "Destination:";

TooltipDefs.FtpConfigurationDetails = [
    { name: "Host", description: "The address of the FTP or SFTP server, e.g. yourcompany.com", subsection: [] },
    { name: "Protocol", description: "Specifies if the standard File Transfer Protocol (FTP) or the SSH File Transfer Protocol (SFTP) should be used.", subsection: [] },
    { name: "Port", description: "The port of the server to connect to.", subsection: [] },
    { name: "Username", description: "If required for your connection, specifies the username to use for authenticating. Leave blank to use anonymous.", subsection: [] },
    { name: "Password", description: "If required for your connection, specifies the password to use for authentication. Leave blank to use anonymous.", subsection: [] },
    {
        name: "CSV filepath",
        description: "The location of the CSV file to be imported from the FTP/SFTP. " +
        "If you set the generated CSV file to always include the date, " +
            "you can specify this filepath value to use date wildcards so that the latest file is always imported. " +
            "For example, a filepath of /export/nightlyexport/*yyyy*-*MM*-*dd**_HRIS_export.csv will successfully import the most recently dated file.",
        subsection: []
    }
];

TooltipDefs.FtpConfigurationDetailsTitle = "CSV Import on FTP";

TooltipDefs.RelativityProviderSettingsDetails = [
    {
        name: "Overwrite",
        description: "determines how the system overwrites records once you promote data to the review workspace. This field provides the following choices:",
        subsection: [
            {
                s_name: "Append Only",
                s_description: "promote only new records into the review workspace."
            },
            {
                s_name: "Overlay Only",
                s_description: "update existing records only in the review workspace. Any documents with the same workspace identifier are overlaid."
            },
            {
                s_name: "Append/Overlay ",
                s_description: "adds new records to the review workspace and overlays data on existing records."
            }
        ]
    },
    {
        name: "Multi-Select Field Overlay Behaviour",
        description: "determines how the system overlays records when you promote documents to the review workspace. This field provides the following choices:",
        subsection: [
            {
                s_name: "Merge Values ",
                s_description: "merges all values for multi-choice and multi-object fields in the source data" +
                    " with corresponding multi-choice and multi-option fields in the workspace, " +
                    "regardless of the overlay behaviour settings in the environment."
            },
            {
                s_name: "Replace Values",
                s_description: "replaces all values for multi-choice and multi-object fields in the source data " +
                    "with corresponding multi-choice and multi-option fields in the workspace, " +
                    "regardless of the overlay behaviour settings in the environment."
            },
            {
                s_name: "Use Field Settings",
                s_description: "merges or replaces all values for multi-choice and multi-object fields in the source data " +
                    "with corresponding multi-choice and multi-option fields in the workspace according to the overlay behaviour settings in the environment."
            }
        ]
    },
    {
        name: "Copy Native Files",
        description: "allows you to indicate whether Integration Points copies any native files while syncing data between the source and destination workspaces.",
        subsection: [
            {
                s_name: "Physical Files",
                s_description: "when you select this option physical natives files are transferred."
            },
            {
                s_name: "Links Only",
                s_description: "select this option if you want only reference links to be set to the source location of natives."
            },
            {
                s_name: "Select No",
                s_description: "if you don’t want to copy natives. When this option is selected only documents will be copied."
            }
        ]
    },
    {
        name: "Copy Images",
        description: "determines whether images are copied while syncing data between the source and destination workspaces. " +
            "When set to Yes, only image links are transferred to the destination case file table. Physical image files are only copied to the destination case's fileshare when the Copy Files to Repository setting is also set to Yes.",
        subsection: []
    }, {
        name: "Image Precedence",
        description: "use this drop-down to indicate whether you want to transfer Original Images or Production Images to your destination workspace. " +
            "For Production Images, you’ll need to indicate a precedence in the Production Precedence field below.",
        subsection: []
    },
    {
        name: "Use Folder Path Information",
        description: "allows you to use a metadata field to build the folder structure for the documents that you promote to the review workspace:",
        subsection: [
            {
                s_name: "Read From Field",
                s_description: "select this to use a metadata field to build the folder structure for the documents that you promote to the destination/review workspace"
            },
            {
                s_name: "Read From Folder Tree",
                s_description: "select this to use the existing folder tree in the Documents tab to build the folder structure " +
                    "for the documents that you promote to the destination/review workspace."
            },
            {
                s_name: "Select No",
                s_description: "if you don't want to build a folder structure based on metadata. " +
                    "In this case, Relativity loads all documents directly into the folder indicated by the promote destination, " +
                    "and you create no new folders in the destination workspace."
            }
        ]
    },
    {
        name: "Move Existing Documents",
        description: "",
        subsection: [
            {
                s_name: "Select Yes",
                s_description: "to move existing documents into the folders provided in the Folder Path Information field. " +
                    "Note that if the Folder Path Information field contains an identical folder structure for existing documents in the destination workspace, " +
                    "those documents won’t be moved; instead, they’ll be overlaid with new information."
            },
            {
                s_name: "Select No",
                s_description: "if you don’t want to re-folder existing documents."
            }
        ]
    }
];

TooltipDefs.RelativityProviderSettingsDetailsTitle = "Settings:";
