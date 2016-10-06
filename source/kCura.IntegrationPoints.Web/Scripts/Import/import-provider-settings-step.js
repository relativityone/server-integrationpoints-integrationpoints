var IP = IP || {};

(function (windowObj, root, ko) {
    //Create a new communication object that talks to the host page.
    var message = IP.frameMessaging();

    //preview file btn toggle
    var onClickPreviewFile = function () {
        var content = windowObj.parent.$("#previewFile-content");
        content.hide();
        var btn = windowObj.parent.$("#previewFile");
        btn.click(function () {
            content.toggle();
        });
    }

    //action to launch popul
    var onPreviewFileClick = function () {
        var preFile = windowObj.parent.$("#dd-preivewFile");
        var preError = windowObj.parent.$("#dd-previewErrors");
        var preChoice = windowObj.parent.$("#dd-previewChoiceFolder");
        preFile.on("click", function () {
            console.log("Preview File selected");
            window.open(root.utils.getBaseURL() + '/ImportProvider/ImportPreview/', "_blank", "width=1370, height=795");;
            $(this).close();
            return false;
        });
        preError.click(function () {
            console.log("Preview error has been selected");
        });
        preChoice.click(function () {
            console.log("Preview choice has been selected");
        });
    }
    //create the btn for previewfile
    var addPreviewFilebtn = function () {
        var options = {
            "dd-preivewFile": "Preview File",
            "dd-previewErrors": "Preview Errors",
            "dd-previewChoiceFolder": "Preview Choices & Folders"
        }

        var source = windowObj.parent.$('#progressButtons');
        source.append('<div class="button generic positive"id="previewFile"><span>Preview File</span><i style="float: right;" class="icon-chevron-down"></i></div>');

        var previewFile = windowObj.parent.$("#previewFile");
        previewFile.append('<div id="previewFile-content"></div>');
        var dropdown = windowObj.parent.$("#previewFile-content");

        $.each(options, function (val, text) {
            dropdown.append($('<div id=' + val + '></div>').html(text));
        });
        onClickPreviewFile();
        onPreviewFileClick();
    };

    var ImportTypeModel = function (data) {
        var self = this;

        self.id = ko.observable(data.id);
        self.name = ko.observable(data.name);
        self.value = ko.observable(data.value);
    };

    var viewModel = function () {
        var self = this;
        self.selectedImportType = ko.observable("document");
        self.importTypes = ko.observableArray([
            new ImportTypeModel({ id: "1", value: "document", name: "Document Load File" }),
            new ImportTypeModel({ id: "2", value: "image", name: "Image Load File" }),
            new ImportTypeModel({ id: "3", value: "production", name: "Production Load File" })
        ]);

        this.ProcessingSourceLocationList = ko.observableArray([]);
        this.ProcessingSourceLocationArtifactId = this.ProcessingSourceLocation || 0;
        this.HasBeenRun = ko.observable(self.hasBeenRun || false);
        this.ProcessingSourceLocation = ko.observable(self.ProcessingSourceLocationArtifactId)
            .extend({
                required: true
            });

        this.Fileshare = ko.observable(self.Fileshare).extend({
            required: {
                onlyIf: function () {
                    return self.ProcessingSourceLocation();
                }
            }
        });

        this.toggleLocation = function (enabled) {
            var $el = $("#location-select");
            $el.toggleClass('location-disabled', !enabled);
            $el.children().each(function (i, e) {
                $(e).toggleClass('location-disabled', !enabled);
            });
        };

        self.toggleLocation(false);

        self.locationSelector = new LocationJSTreeSelector();
        //pass in the selectFilesOnly optional parameter so that location-jstree-selector will only allow us to select files
        self.locationSelector.init(self.Fileshare(), [], {
            onNodeSelectedEventHandler: function (node) { self.Fileshare(node.id) },
            selectFilesOnly: true
        });



        $.get(root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationStructure"), function (data) {
            self.ProcessingSourceLocationList(data);
            self.ProcessingSourceLocation(self.ProcessingSourceLocationArtifactId);

            $("#processingSources").change(function (c, item) {
                var artifacId = $("#processingSources option:selected").val();
                var choiceName = $("#processingSources option:selected").text();
                this.getDirectories = $.get(root.utils.generateWebAPIURL("ResourcePool/GetProcessingSourceLocationStructure", artifacId) + '?includeFiles=1')
                    .then(function (result) {
                        self.locationSelector.reload(result);
                        self.toggleLocation(true);
                    })
                    .fail(function (error) {
                        root.message.error.raise("No attributes were returned from the source provider.");
                    });
            });

        });

        var _getModel = function () {
            var model = {
                ImportType: $('#import-importType option:selected').val(),
                //ProcessingSource: windowObj.import.StorageRoot ? windowObj.import.StorageRoot : "",
                //LoadDataFrom: windowObj.import.SelectedFolderPath ? windowObj.import.SelectedFolderPath : "",
                HasStartLine: $("#import-hascolumnnames-checkbox").attr("checked") ? true : false,
                LineNumber: $("#import-columnname-numbers").val(),
                LoadFile: self.Fileshare()
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
                    windowObj.parent.$('#previewFile').remove();
                    //Communicate to the host page that it to continue.
                    this.publish('saveComplete', localModel);
                }
            });

        //An event raised when a user clicks the Back button.
        message.subscribe('back', function () {
            //Execute save logic that persists the root.
            this.publish('saveState', _getModel());
            windowObj.parent.$('#previewFile').remove();
        });

        //An event raised when the host page has loaded the current settings page.
        message.subscribe('load', function (model) {
            addPreviewFilebtn();
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
        });
    };

    ko.applyBindings(new viewModel());
})(this, IP, ko);