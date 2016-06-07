$(function (root) {
    //Create a new communication object that talks to the host page.
    var message = IP.frameMessaging();

    var viewModel;

    //An event raised when the user has clicked the Next or Save button.
    message.subscribe('submit', function () {
        //Execute save logic that persists the state.
        this.publish("saveState", JSON.stringify(ko.toJS(viewModel)));

        if (viewModel.errors().length === 0) {
            //Communicate to the host page that it to continue.
            this.publish('saveComplete', JSON.stringify(viewModel.getSelectedOption()));
        } else {
            viewModel.errors.showAllMessages();
        }

        // Modify destination object to contain target workspaceId
        var destinationJson = IP.frameMessaging().dFrame.IP.points.steps.steps[1].model.destination;
        var destination = JSON.parse(destinationJson);
        destination.CaseArtifactId = viewModel.TargetWorkspaceArtifactId();
        destination.Provider = "Fileshare";
        destination.DoNotUseFieldsMapCache = viewModel.WorkspaceHasChanged;
        destinationJson = JSON.stringify(destination);
        IP.frameMessaging().dFrame.IP.points.steps.steps[1].model.destination = destinationJson;
    });

    //An event raised when a user clicks the Back button.
    message.subscribe('back', function () {
        //Execute save logic that persists the state.
        this.publish('saveState', JSON.stringify(ko.toJS(viewModel)));
    });

    //An event raised when the host page has loaded the current settings page.
    message.subscribe('load', function (m) {
        var _bind = function (m) {
            viewModel = new Model(m);
            ko.applyBindings(viewModel, document.getElementById('exportProviderConfiguration'));
        }

        // expect model to be serialized to string
        if (typeof m === "string") {
            try {
                m = JSON.parse(m);
            } catch (e) {
                m = undefined;
            }
            _bind(m);
        } else {
            _bind({});
        }
    });

    var Model = function (m) {

        var state = $.extend({}, {}, m);
        var self = this;

        this.workspaces = ko.observableArray(state.workspaces || []);
        this.savedSearches = ko.observableArray(state.savedSearches || []);

        this.disable = IP.frameMessaging().dFrame.IP.points.steps.steps[0].model.hasBeenRun();

        this.Fileshare = ko.observable(state.Fileshare).extend({
            required: true
        });

        this.IncludeNativeFilesPath = ko.observable(state.IncludeNativeFilesPath || "true");

        this.dataFileFormats = [
          { key: "Concordance (.dat)", value: "Concordance" },
          { key: "HTML (.html)", value: "HTML" },
          { key: "Comma-separated (.csv)", value: "CSV" },
          { key: "Custom (.txt)", value: "Custom" }
        ];

        this.SelectedDataFileFormat = ko.observable(state.SelectedDataFileFormat).extend({
            required: true
        });

        this.ColumnSeparator = ko.observable(state.ColumnSeparator).extend({
            required: {
                onlyIf: function () {
                    return self.SelectedDataFileFormat() === "Custom";
                }
            }
        });
        this.QuoteSeparator = ko.observable(state.QuoteSeparator).extend({
            required: {
                onlyIf: function () {
                    return self.SelectedDataFileFormat() === "Custom";
                }
            }
        });
        this.NewlineSeparator = ko.observable(state.NewlineSeparator).extend({
            required: {
                onlyIf: function () {
                    return self.SelectedDataFileFormat() === "Custom";
                }
            }
        });
        this.MultiValueSeparator = ko.observable(state.MultiValueSeparator).extend({
            required: {
                onlyIf: function () {
                    return self.SelectedDataFileFormat() === "Custom";
                }
            }
        });
        this.NestedValueSeparator = ko.observable(state.NestedValueSeparator).extend({
            required: {
                onlyIf: function () {
                    return self.SelectedDataFileFormat() === "Custom";
                }
            }
        });

        this.isCustom = ko.observable(false);
        this.isCustomDisabled = ko.observable(true);

        this.separatorsList = function () {
            var result = [];
            for (var i = 0; i < 256; i++) {
                result.push({ key: String.fromCharCode(i) + " (ASCII:" + i + ")", value: i });
            }
            return result;
        }();

        this.SelectedDataFileFormat.subscribe(function (value) {
            //default values have been taken from RDC application
            if (value === 'Concordance') {
                self.ColumnSeparator(20);
                self.QuoteSeparator(254);
                self.NewlineSeparator(174);
                self.MultiValueSeparator(59);
                self.NestedValueSeparator(92);
            }
            if (value === 'CSV') {
                self.ColumnSeparator(44);
                self.QuoteSeparator(34);
                self.NewlineSeparator(10);
                self.MultiValueSeparator(59);
                self.NestedValueSeparator(92);
            }
        });

        this.SelectedDataFileFormat.subscribe(function (value) {
            self.isCustom(value === 'Custom');
            if (value === 'Custom') {
                self.isCustomDisabled(undefined);
            } else {
                self.isCustomDisabled(true);
            }
        });

        this.CopyFileFromRepository = ko.observable(state.CopyFileFromRepository || "false");
        this.OverwriteFiles = ko.observable(state.OverwriteFiles || "false");

        this.TargetWorkspaceArtifactId = ko.observable(state.TargetWorkspaceArtifactId).extend({
            required: true
        });

        this.TargetWorkspaceArtifactId.subscribe(function (value) {
            if (self.TargetWorkspaceArtifactId !== value) {
                self.WorkspaceHasChanged = true;
            }
        });

        this.SavedSearchArtifactId = ko.observable(state.SavedSearchArtifactId);

        this.SavedSearch = ko.observable(state.SavedSearch).extend({
            required: true
        });

        this.updateSelectedSavedSearch = function () {
            var selectedSavedSearch = ko.utils.arrayFirst(self.savedSearches(), function (item) {
                if (item.value === self.SavedSearchArtifactId()) {
                    return item;
                }
            });

            self.SavedSearch(selectedSavedSearch);
        }

        if (self.savedSearches().length === 0) {
            // load saved searches
            IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('SavedSearchFinder') }).then(function (result) {
                self.savedSearches(result);
                self.updateSelectedSavedSearch();
            });
        } else {
            self.updateSelectedSavedSearch();
        }

        if (self.workspaces().length === 0) {
            // load workspaces
            IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('workspaceFinder') }).then(function (result) {
                self.workspaces(result);
            });
        }

        this.DataFileEncodingType = ko.observable(state.DataFileEncodingType).extend({
            required: true
        });

        this.updateSelectedDataFileEncodingType = function (value) {
            var selectedDataFileEncodingType = ko.utils.arrayFirst(self.DataFileEncodingTypeList(), function (item) {
                return item.name === value;
            });

            self.DataFileEncodingType(selectedDataFileEncodingType.name);
        }

        this.DataFileEncodingTypeList = ko.observableArray([]);
        if (self.DataFileEncodingTypeList.length === 0) {
            IP.data.ajax({ type: 'get', url: IP.utils.generateWebAPIURL('GetAvailableEncodings') }).then(function (result) {

                // By default user should see only 4 default options: Unicode, Unicode (Big-Endian), Unicode (UTF-8), Western European (Windows) as in RDC
                self.DataFileEncodingTypeList(ko.utils.arrayFilter(result,
                    function (item) {
                        return $.inArray(item.name, ['utf-16', 'utf-16BE', 'utf-8', 'Windows-1252']) >= 0;
                    })
                );
                self.updateSelectedDataFileEncodingType(state.DataFileEncodingType);
            });
        }
        else {
            self.updateSelectedDataFileEncodingType(state.DataFileEncodingType);
        }

        this.ExportImagesChecked = ko.observable(state.ExportImagesChecked || "false").extend({
            required: true
        });

        this.imageFileTypes = ko.observableArray([
            { key: 0, value: "Single page TIFF/JPEG" },
            { key: 1, value: "Multi page TIFF/JPEG" },
            { key: 2, value: "PDF" }
        ]);

        this.SelectedImageFileType = ko.observable(state.SelectedImageFileType).extend({
            required: {
                onlyIf: function () {
                    return self.ExportImagesChecked() === "true";
                }
            }
        });

        this.errors = ko.validation.group(this, { deep: true });

        this.getSelectedOption = function () {
            return {
                "SavedSearchArtifactId": self.SavedSearch().value,
                "SavedSearch": self.SavedSearch().displayName,
                "TargetWorkspaceArtifactId": self.TargetWorkspaceArtifactId(),
                "SourceWorkspaceArtifactId": IP.utils.getParameterByName('AppID', window.top),
                "CopyFileFromRepository": self.CopyFileFromRepository(),
                "OverwriteFiles": self.OverwriteFiles(),
                "Fileshare": self.Fileshare(),
                "ExportImagesChecked": self.ExportImagesChecked(),
                "SelectedImageFileType": self.ExportImagesChecked() === "true" ? self.SelectedImageFileType() : "",
                "IncludeNativeFilesPath": self.IncludeNativeFilesPath(),
                "SelectedDataFileFormat": self.SelectedDataFileFormat(),
                "DataFileEncodingType": self.DataFileEncodingType(),
                "ColumnSeparator": self.ColumnSeparator(),
                "QuoteSeparator": self.QuoteSeparator(),
                "NewlineSeparator": self.NewlineSeparator(),
                "MultiValueSeparator": self.MultiValueSeparator(),
                "NestedValueSeparator": self.NestedValueSeparator()
            }
        }
    }
});
