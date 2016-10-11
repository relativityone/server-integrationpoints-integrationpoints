'use strict';
(function (windowObj, root, ko) {

    var ImportTypeModel = function (data) {
        var self = this;

        self.id = ko.observable(data.id);
        self.name = ko.observable(data.name);
        self.value = ko.observable(data.value);
    };

    var viewModel = function () {
        //TODO: refactor viewmodel
        //- make self / this usage consistent
        //- consider making selectedImportType a computed observable that indexes into importTypes, and pulling the string from the 'value' field. would need a new variable like 'activeImportType'
        //- assignment to  ProcessingSourceLocationArtifactId can never be true, so always defaults to 0; why is the || used?
        //- assignment to HasBeenRun references a non-existent self.hasBeenRun; again, can never be present so always defaults to false
        //- why are we passing self.Fileshare into the ko.observable function that defines... this.Fileshare? understand required / onlyIf; is this knockout or integration points?

        var self = this;
        self.selectedImportType = ko.observable("document");
        self.importTypes = ko.observableArray([
            new ImportTypeModel({ id: "1", value: "document", name: "Document Load File" }),
            new ImportTypeModel({ id: "2", value: "image", name: "Image Load File" }),
            new ImportTypeModel({ id: "3", value: "production", name: "Production Load File" })
        ]);

        self.asciiDelimiters = ko.observableArray([]);
        self.setAsciiDelimiters = function(data) {
            self.asciiDelimiters(data);
        };

        self.ProcessingSourceLocationList = ko.observableArray([]);
        self.ProcessingSourceLocationArtifactId = this.ProcessingSourceLocation || 0;
        self.HasBeenRun = ko.observable(self.hasBeenRun || false);
        self.ProcessingSourceLocation = ko.observable(self.ProcessingSourceLocationArtifactId)
            .extend({
                required: true
            });

        self.Fileshare = ko.observable(self.Fileshare).extend({
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
