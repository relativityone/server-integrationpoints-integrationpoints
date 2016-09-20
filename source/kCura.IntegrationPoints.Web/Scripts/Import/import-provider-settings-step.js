(function (windowObj) {

    //Create a new communication object that talks to the host page.
    var message = IP.frameMessaging();

    var _getModel = function () {
        var model = {
            ImportType: $("input:radio[name=import-provider-import-detail-radio]").val(),
            ProcessingSource: windowObj.import.StorageRoot,
            LoadDataFrom: windowObj.import.SelectedFolderPath,
            HasStartLine: $("#import-hascolumnnames-checkbox").attr("checked") ? true : false,
            LineNumber: $("#import-columnname-numbers").val(),
            LoadFile: $("#input-loadFile-text").val()
        };

        console.log(model);
        return JSON.stringify(model);
    };

    //An event raised when the user has clicked the Next or Save button.
    message.subscribe('submit', function () {
        //Execute save logic that persists the state.
        var localModel = _getModel();
        var parsedModel = JSON.parse(localModel);

        this.publish("saveState", localModel);

        if (parsedModel.CsvFilePath === '') {
            IP.frameMessaging().dFrame.IP.message.error.raise('Please select a load file to continue.');
        } else {
            //Communicate to the host page that it to continue.
            this.publish('saveComplete', localModel);
        }
    });

    ////An event raised when a user clicks the Back button.
    //message.subscribe('back', function () {
    //    //Execute save logic that persists the state.
    //    this.publish('saveState', _getModel());
    //});

    ////An event raised when the host page has loaded the current settings page.
    //message.subscribe('load', function (model) {
    //    if (model != '') {
    //        var sourceConfig = JSON.parse(model);
    //        windowObj.o365.StorageRoot = sourceConfig.StorageRoot;
    //        windowObj.o365.SelectedFolderPath = sourceConfig.CsvFilePath;
    //    }
    //    else {
    //        windowObj.o365.StorageRoot = '';
    //        windowObj.o365.SelectedFolderPath = '';
    //    }
    //    windowObj.o365.IPFrameMessagingLoadEvent = true;
    //});

})(this);