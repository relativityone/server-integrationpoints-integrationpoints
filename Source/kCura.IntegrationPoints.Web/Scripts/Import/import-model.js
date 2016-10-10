'use strict';
(function (windowObj, root, ko) {

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

    }
    windowObj.RelativityImport.koModel = new viewModel();
    ko.applyBindings(windowObj.RelativityImport.koModel);

})(this, IP, ko);
