var ExportEnums = {};

ExportEnums.DataFileFormats = [
  { value: 0, key: "Concordance (.dat)" },
  { value: 1, key: "HTML (.html)" },
  { value: 2, key: "Comma-separated (.csv)" },
  { value: 3, key: "Custom (.txt)" }
];

ExportEnums.DataFileFormatEnum = {
    Concordance: 0,
    HTML: 1,
    CSV: 2,
    Custom: 3
};

ExportEnums.ImageDataFileFormats = [
  { value: 0, key: "Opticon" },
  { value: 1, key: "IPRO" },
  { value: 2, key: "IPRO (FullText)" }
];

ExportEnums.ImageFileTypes = [
  { value: 0, key: "Single page TIFF/JPEG" },
  { value: 1, key: "Multi page TIFF/JPEG" },
  { value: 2, key: "PDF" }
];

ExportEnums.FilePathType = [
  { value: 0, key: "Relative" },
  { value: 1, key: "Absolute" },
  { value: 2, key: "User Prefix" }
];

ExportEnums.FilePathTypeEnum = {
    Relative: 0,
    Absolute: 1,
    UserPrefix: 2
};

ExportEnums.ProductionPrecedenceType = [
  { value: 0, key: "Original Images" },
  { value: 1, key: "Produced Images" }
];

ExportEnums.ProductionPrecedenceTypeEnum = {
	Original: 0,
	Produced: 1
};