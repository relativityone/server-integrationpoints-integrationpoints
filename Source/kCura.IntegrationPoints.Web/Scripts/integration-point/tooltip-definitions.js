﻿var TooltipDefs = {}

TooltipDefs.ExportDetails = [
	{
		name: "Export Type", description: "this field reflects the type of export job you're running, which you specified on the Setup layout. By default, Load File is selected as the base configuration for your export job. You also have the option of enhancing the load file through by selecting Images, Natives, and Text Fields as Files, depending on the makeup of your data. Selecting any of these makes additional corresponding output settings available.",
		subsection: []
	},
	{
		name: "Destination Folder", description: "specify the folder into which you want to export the data from the load file. This reads the Default File Repository defined for the Workspace and allows you to select a subfolder of that location. Contact Sys Admin for more details.",
		subsection: []
	}
];

TooltipDefs.ExportDetailsTitle = "Export Details:";

TooltipDefs.RelativityProviderSettingsDetails = [
	{
		name: "Append Only",
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
				s_description: "merges all values for multi-choice and multi-object fields in the source data with corresponding multi-choice and multi-option fields in the workspace, regardless of the overlay behaviour settings in the environment."
			},
			{
				s_name: "Replace Values",
				s_description: "replaces all values for multi-choice and multi-object fields in the source data with corresponding multi-choice and multi-option fields in the workspace, regardless of the overlay behaviour settings in the environment."
			},
			{
				s_name: "Use Field Settings",
				s_description: "merges or replaces all values for multi-choice and multi-object fields in the source data with corresponding multi-choice and multi-option fields in the workspace according to the overlay behaviour settings in the environment."
			}
		]
	},
	{
		name: "Copy Native File",
		description: "allows you to indicate whether Integration Points copies any native files while syncing data between the source and destination workspaces.",
		subsection: []
	},
	{
		name: "Use Folder Path Information",
		description: "determines how the system overlays records when you promote documents to the review workspace. This field provides the following choices:",
		subsection: [
			{
				s_name: "Select Yes",
				s_description: "to use a metadata field to build the folder structure for the documents that you promote to the review workspace."
			},
			{
				s_name: "Select No",
				s_description: "if you don't want to build a folder structure based on metadata. In this case, Relativity loads all documents directly into the folder indicated by the promote destination, and you create no new folders in the destination workspace."
			}
		]
	},
	{
		name: "Move Existing Documents",
		description: "setting this option to Yes will result in moving existing documents in the destination location into folders provided in Folder Path Information field, if different. Setting this option to No will not re-folder existing documents.",
		subsection: []
	}
];

TooltipDefs.RelativityProviderSettingsDetailsTitle = "Settings:";