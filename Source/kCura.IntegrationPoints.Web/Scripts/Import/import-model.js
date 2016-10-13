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

        self.startLine = ko.observable("1");

        self.fileContainsColumn = ko.observable("true");

        self.asciiDelimiters = ko.observableArray([]);
        self.setAsciiDelimiters = function (data) {
            self.asciiDelimiters(data);
        };

        self.selectedColumnAsciiDelimiter = ko.observable();
        self.setSelectedColumnAsciiDelimiters = function (data) {
            self.selectedColumnAsciiDelimiter(data);
        };

        self.selectedQuoteAsciiDelimiter = ko.observable();
        self.setSelectedQuoteAsciiDelimiters = function (data) {
            self.selectedQuoteAsciiDelimiter(data);
        };

        self.selectedNewLineAsciiDelimiter = ko.observable();
        self.setSelectedNewLineAsciiDelimiters = function (data) {
            self.selectedNewLineAsciiDelimiter(data);
        };

        self.selectedMultiLineAsciiDelimiter = ko.observable();
        self.setSelectedMultiLineAsciiDelimiters = function (data) {
            self.selectedMultiLineAsciiDelimiter(data);
        };

        self.selectedNestedValueAsciiDelimiter = ko.observable();
        self.setSelectedNestedValueAsciiDelimiters = function (data) {
            self.selectedNestedValueAsciiDelimiter(data);
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

        self.DataFileEncodingTypeValue = "Select...";

        self.DataFileEncodingType = ko.observable(self.DataFileEncodingTypeValue).extend({
            required: true
        });

        //Populate file encoding dropdown
        self.FileEncodingTypeList = ko.observableArray([]);
        self._UpdateFileEncodingTypeList = function () {
            IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('GetAvailableEncodings') }).then(function (result) {
                function Group(label, children) {
                    this.label = ko.observable(label);
                    this.children = ko.observableArray(children);
                };
                function Option(displayName, name) {
                    this.displayName = ko.observable(displayName);
                    this.name = ko.observable(name);
                };

                var favorite = [];
                var others = [];

                for (var i = 0; i < result.length; i++) {
                    var option = new Option(result[i].displayName, result[i].name);

                    if ($.inArray(result[i].name, ['utf-16', 'utf-16BE', 'utf-8', 'Windows-1252']) >= 0) {
                        favorite.push(option);
                    } else {
                        others.push(option);
                    }
                }

                // By default user should see only 4 default options: Unicode, Unicode (Big-Endian), Unicode (UTF-8), Western European (Windows) as in RDC
                //self.FileEncodingTypeList([new Group("", [new Option("Select...", "")]), new Group("Favorite", favorite), new Group("Others", others)]);

                self.FileEncodingTypeList([new Group("", [new Option("Unicode (UTF-8)", "utf-8")]), new Group("Favorite", favorite), new Group("Others", others)]);

                self.DataFileEncodingType(self.DataFileEncodingTypeValue);
                self.DataFileEncodingType.isModified(false);
            });
        }
        self._UpdateFileEncodingTypeList();

    }
    windowObj.RelativityImport.koModel = new viewModel();
    ko.applyBindings(windowObj.RelativityImport.koModel);

})(this, IP, ko);
