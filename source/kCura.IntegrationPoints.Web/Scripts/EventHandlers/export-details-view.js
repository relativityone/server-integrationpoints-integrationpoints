var ExportDetailsView = {};

IP.isFileshareProvider = true;

ExportDetailsView.downloadSummaryPage = function () {
    var dataContainer = new DataContainer();
    dataContainer.hideContainer();

    IP.data.ajax({
        url: IP.utils.generateWebURL('Fileshare', 'ExportDetails'),
        type: 'get',
        dataType: 'html'
    }).then(function (result) {
        IP.utils.getViewField(IP.nameId).closest('.innerTabTable').replaceWith(result);

        var viewModel = new Model(dataContainer);
        ko.applyBindings(viewModel, document.getElementById('exportSummaryPage'));
    });
}

var DataContainer = function () {
    this.hasErrors = IP.utils.getViewField(IP.hasErrorsId).siblings('.dynamicViewFieldValue').text();
    this.logErrors = IP.utils.getViewField(IP.logErrorsId).siblings('.dynamicViewFieldValue').text();
    this.emailNotification = IP.utils.getViewField(IP.emailNotificationId).siblings('.dynamicViewFieldValue').text();
    this.name = IP.utils.getViewField(IP.nameId).siblings('.dynamicViewFieldValue').text();
    this.settings = JSON.parse(IP.utils.getViewField(IP.sourceConfiguration).siblings('.dynamicViewFieldValue').text());

    this.hideContainer = function () {
        IP.utils.getViewField(IP.sourceConfiguration).parent().hide();
        IP.utils.getViewField(IP.overwriteFieldsId).parent().hide();
        IP.utils.getViewField(IP.sourceProviderId).parent().hide();
        IP.utils.getViewField(IP.destinationid).parent().hide();
        IP.utils.getViewField(IP.destinationProviderid).parent().hide();
    };
}

var Model = function (dataContainer) {
    var self = this;

    this.hasErrors = dataContainer.hasErrors;
    this.logErrors = dataContainer.logErrors;
    this.emailNotification = dataContainer.emailNotification;
    this.name = dataContainer.name;
    this.settings = dataContainer.settings;

    this.volumeInfo = function () {
        return self.settings.VolumePrefix + "; " + self.settings.VolumeStartNumber + "; " + self.settings.VolumeDigitPadding + "; " + self.settings.VolumeMaxSize;
    };

    this.subdirectoryInfo = function () {
        return self.settings.SubdirectoryNativePrefix + "; " + self.settings.SubdirectoryImagePrefix + "; " + self.settings.SubdirectoryTextPrefix + "; " + self.settings.SubdirectoryStartNumber + "; " + self.settings.SubdirectoryDigitPadding + "; " + self.settings.SubdirectoryMaxFiles
    };

    this.exportType = function () {
        return "Load file"
        + (self.settings.ExportImagesChecked ? "; Images" : "")
        + (self.settings.CopyFileFromRepository ? "; Natives" : "");
    };

    this.filePath = function () {
        var filePathType = "";
        for (var i = 0; i < ExportEnums.FilePathType.length; i++) {
            if (ExportEnums.FilePathType[i].value == self.settings.FilePath) {
                filePathType = ExportEnums.FilePathType[i].key;
            }
        }
        return (self.settings.IncludeNativeFilesPath ? "Include" : "Do not include")
        + ("; " + filePathType)
        + (self.settings.FilePath == ExportEnums.FilePathTypeEnum.UserPrefix ? (": " + self.settings.UserPrefix) : "");
    };

    this.loadFileInfo = function () {
        var fileFormat = "";
        for (var i = 0; i < ExportEnums.DataFileFormats.length; i++) {
            if (ExportEnums.DataFileFormats[i].value == self.settings.SelectedDataFileFormat) {
                fileFormat = ExportEnums.DataFileFormats[i].key;
            }
        }
        return fileFormat + "; " + self.settings.DataFileEncodingType.toUpperCase();
    };

    this.imageFileType = function () {
        for (var i = 0; i < ExportEnums.ImageFileTypes.length; i++) {
            if (ExportEnums.ImageFileTypes[i].value == self.settings.SelectedImageFileType) {
                return ExportEnums.ImageFileTypes[i].key;
            }
        }
    };

    this.imageDataFileFormat = function () {
        for (var i = 0; i < ExportEnums.ImageDataFileFormats.length; i++) {
            if (ExportEnums.ImageDataFileFormats[i].value == self.settings.SelectedImageDataFileFormat) {
                return ExportEnums.ImageDataFileFormats[i].key;
            }
        }
    };
};

$(function () {
    ExportDetailsView.downloadSummaryPage();
});
