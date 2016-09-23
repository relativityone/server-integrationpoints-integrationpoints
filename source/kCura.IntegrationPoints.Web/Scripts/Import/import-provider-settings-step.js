var IP = IP || {};

(function (windowObj, root, ko) {
    //Create a new communication object that talks to the host page.
    var message = IP.frameMessaging();

    var viewModel = function () {

        var self = this;

        self.ImportTypeChoiceValue = ko.observable();
        self.ImportTypeChoiceValue("document");

        this.ProcessingSourceLocationList = ko.observableArray([]);
        this.ProcessingSourceLocationArtifactId = this.ProcessingSourceLocation || 0;

        this.ProcessingSourceLocation = ko.observable(self.ProcessingSourceLocationArtifactId)
            .extend({
                required: true
            });


        $.get(root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationStructure"), function (data) {
            self.ProcessingSourceLocationList(data);
        });

        var _getModel = function () {
            var model = {
                InputType: $('input:radio[name=import-type]:checked').val(),
                //ProcessingSource: windowObj.import.StorageRoot ? windowObj.import.StorageRoot : "",
                //LoadDataFrom: windowObj.import.SelectedFolderPath ? windowObj.import.SelectedFolderPath : "",
                HasStartLine: $("#import-hascolumnnames-checkbox").attr("checked") ? true : false,
                LineNumber: $("#import-columnname-numbers").val(),
                LoadFile: $("#import-loadFile-text").val()
            };

            console.log(model);
            return JSON.stringify(model);
        };

        //An event raised when the user has clicked the Next or Save button.
        message.subscribe('submit',
            function () {
                //Execute save logic that persists the root.
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
        //    //Execute save logic that persists the root.
        //    this.publish('saveState', _getModel());
        //});

        ////An event raised when the host page has loaded the current settings page.
        //message.subscribe('load', function (model) {
        //if (model != '') {
        //    var sourceConfig = JSON.parse(model);
        //    windowObj.import.StorageRoot = sourceConfig.StorageRoot;
        //    windowObj.import.SelectedFolderPath = sourceConfig.CsvFilePath;
        //}
        //else {
        //    windowObj.import.StorageRoot = "";
        //    windowObj.import.SelectedFolderPath = "";
        //}
        //windowObj.import.IPFrameMessagingLoadEvent = true;
        //});
    };

    ko.applyBindings(new viewModel());
})(this, IP, ko);