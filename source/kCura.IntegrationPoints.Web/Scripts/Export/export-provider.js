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

        this.SelectedDestinationPath = ko.observable(state.Fileshare).extend({
            required: true
        });

        this.IncludeNativeFilesPath = ko.observable(state.IncludeNativeFilesPath || "true");

        this.dataFileFormats = [
          { key: "Concordance (.dat)", value: "Concordance" },
          { key: "HTML (.html)", value: "HTML" },
          { key: "Comma-separated (.csv)", value: "CSV" }
          //{key: "Custom (.txt)", value: "Custom"}
        ];

        this.SelectedDataFileFormat = ko.observable(state.SelectedDataFileFormat).extend({
            required: true
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

        this.ExportImagesChecked = ko.observable(state.ExportImagesChecked || "false").extend({
            required: true
        });

        this.imageFileTypes = ko.observableArray([
            { key: 0, value: "Single page TIFF/JPEG" },
            { key: 1, value: "Multi page TIFF/JPEG" },
            { key: 2, value: "PDF" }
        ]);

        this.SelectedImageFileType = ko.observable().extend({
            required: true
        });

        this.updateSelectedImageFileType = function (value) {
            var fileType = ko.utils.arrayFirst(self.imageFileTypes(), function (item) {
                if (item.key === value) {
                    return item;
                }
            });

            self.SelectedImageFileType(fileType);
        };

        self.updateSelectedImageFileType(state.SelectedImageFileType);

        this.errors = ko.validation.group(this, { deep: true });

        this.getSelectedOption = function () {
            return {
                "SavedSearchArtifactId": self.SavedSearch().value,
                "SavedSearch": self.SavedSearch().displayName,
                "TargetWorkspaceArtifactId": self.TargetWorkspaceArtifactId(),
                "SourceWorkspaceArtifactId": IP.utils.getParameterByName('AppID', window.top),
                "CopyFileFromRepository": self.CopyFileFromRepository(),
                "OverwriteFiles": self.OverwriteFiles(),
                "Fileshare": self.SelectedDestinationPath(),
                "ExportImagesChecked": self.ExportImagesChecked(),
                "SelectedImageFileType": self.SelectedImageFileType().key,
                "IncludeNativeFilesPath": self.IncludeNativeFilesPath(),
                "SelectedDataFileFormat": self.SelectedDataFileFormat()
            }
        }
    }
});
