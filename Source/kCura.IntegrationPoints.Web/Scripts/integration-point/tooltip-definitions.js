var TooltipDefs = {}

TooltipDefs.ExportDetails = [
	{ name: "Export Type", description: "this field reflects the type of export job you're running, which you specified on the Setup layout. By default, Load File is selected as the base configuration for your export job. You also have the option of enhancing the load file through by selecting Images, Natives, and Text Fields as Files, depending on the makeup of your data. Selecting any of these makes additional corresponding output settings available." },
	{ name: "Destination Folder", description: "specify the folder into which you want to export the data from the load file. This reads the Default File Repository defined for the Workspace and allows you to select a subfolder of that location. Contact Sys Admin for more details." }
];

TooltipDefs.ExportDetailsTitle = "Export Details:";


TooltipDefs.FtpConfigurationDetails = [
	{ name: "Host", description: "The address of the FTP or SFTP server, e.g. yourcompany.com" },
	{ name: "Protocol", description: "Specifies if the standard File Transfer Protocol (FTP) or the SSH File Transfer Protocol (SFTP) should be used." },
	{ name: "Port", description: "The port of the server to connect to." },
	{ name: "Username", description: "If required for your connection, specifies the username to use for authenticating. Leave blank to use anonymous." },
	{ name: "Password", description: "If required for your connection, specifies the password to use for authentication. Leave blank to use anonymous." },
	{ name: "CSV filepath", description: "The location of the CSV file to be imported from the FTP/SFTP. If you set the generated CSV file to always include the date, you can specify this filepath value to use date wildcards so that the latest file is always imported. For example, a filepath of /export/nightlyexport/*yyyy*-*MM*-*dd**_HRIS_export.csv will successfully import the most recently dated file." }
];

TooltipDefs.FtpConfigurationDetailsTitle = "CSV Import on FTP";