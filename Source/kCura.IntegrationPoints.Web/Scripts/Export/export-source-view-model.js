var ExportSourceViewModel = function (state) {
    var self = this;
    var sourceConfiguration = state.sourceConfiguration || {};

    self.Cache = state.cache;

    self.HasBeenRun = ko.observable(state.hasBeenRun);

    self.TypeOfExport = ko.observable((sourceConfiguration.ExportType === parseInt(sourceConfiguration.ExportType)) ? sourceConfiguration.ExportType : ExportEnums.SourceOptionsEnum.SavedSearch);

    // saved searches

    self.SavedSearches = ko.observableArray();

    self.IsSavedSearchSelected = function () {
        return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.SavedSearch;
    };

    self.SavedSearchArtifactId = ko.observable(sourceConfiguration.SavedSearchArtifactId).extend({
        required: {
            onlyIf: function () {
                return self.IsSavedSearchSelected();
            }
        }
    });

    self.SavedSearchesTree = ko.observable();

    self.GetSelectedSavedSearch = function (artifactId) {
        var selectedSavedSearch = ko.utils.arrayFirst(self.SavedSearches(), function (item) {
            if (item.value === artifactId) {
                return item;
            }
        });

        return selectedSavedSearch;
    };

    self.IsSavedSearchTreeNode = function (node) {
        return !!node && (node.icon === "jstree-search" || node.icon === "jstree-search-personal");
    }

    var savedSearchPickerViewModel = new SavedSearchPickerViewModel(function (value) {
        self.SavedSearchArtifactId(value.id);
    }, self.IsSavedSearchTreeNode);

    Picker.create("savedSearchPicker", "SavedSearchPicker", savedSearchPickerViewModel);

    self.OpenSavedSearchPicker = function () {
        savedSearchPickerViewModel.open(self.SavedSearchesTree(), self.SavedSearchArtifactId());
    };

    self.GetSavedSearches = function (tree) {
        var _searches = [];
        var _iterate = function (node, depth) {
            if (self.IsSavedSearchTreeNode(node)) {
                _searches.push({
                    value: node.id,
                    displayName: node.text
                });
            }

            for (var i = 0, len = node.children.length; i < len; i++) {
                _iterate(node.children[i], depth + 1);
            }
        };

        _iterate(tree, 0);

        return _searches;
    };

    self.UpdateSelectedSavedSearch = function (artifactId) {
        var selectedSearch = self.GetSelectedSavedSearch(artifactId);

        if (!!selectedSearch) {
            self.SavedSearchArtifactId(selectedSearch.value);
        } else {
            self.SavedSearchArtifactId(undefined);
        }
    };

    self.UpdateSavedSearches = function (artifactId) {
        self.SavedSearchesTree(self.Cache.SavedSearchesResult);
        self.SavedSearches(self.GetSavedSearches(self.Cache.SavedSearchesResult));
        self.UpdateSelectedSavedSearch(artifactId || self.SavedSearchArtifactId());
    };

    // folders and subfolders

    self.FolderLabelDescription = ko.observable();

    self.IsFolderOrSubfolderSelected = function () {
        var isFolderOrSubfolderSelected = false;

        if (self.TypeOfExport() === ExportEnums.SourceOptionsEnum.Folder) {
            self.FolderLabelDescription(ExportEnums.SourceOptions[ExportEnums.SourceOptionsEnum.Folder].key);
            isFolderOrSubfolderSelected = true;
        }

        if (self.TypeOfExport() === ExportEnums.SourceOptionsEnum.FolderSubfolder) {
            self.FolderLabelDescription(ExportEnums.SourceOptions[ExportEnums.SourceOptionsEnum.FolderSubfolder].key);
            isFolderOrSubfolderSelected = true;
        }

        return isFolderOrSubfolderSelected;
    };

    self.FolderArtifactId = ko.observable(sourceConfiguration.FolderArtifactId).extend({
        required: {
            onlyIf: function () {
                return self.IsFolderOrSubfolderSelected();
            }
        }
    });

    self.FolderArtifactName = ko.observable(sourceConfiguration.FolderArtifactName).extend({
        required: {
            onlyIf: function () {
                return self.IsFolderOrSubfolderSelected();
            }
        }
    });

    self.Folders = ko.observable()

    self.GetFolderFullName = function (currentFolder, folderId) {
        currentFolder = currentFolder || self.Folders();
        folderId = folderId || self.FolderArtifactId();

        if (currentFolder.id === folderId) {
            return currentFolder.text;
        } else {
            for (var i = 0; i < currentFolder.children.length; i++) {
                var childFolderPath = self.GetFolderFullName(currentFolder.children[i], folderId);
                if (childFolderPath !== "") {
                    return currentFolder.text + "/" + childFolderPath;
                }
            }
        }

        return "";
    };

    // views

    self.AvailableViews = ko.observableArray();

    self.ViewId = ko.observable(sourceConfiguration.ViewId).extend({
        required: {
            onlyIf: function () {
                return self.IsFolderOrSubfolderSelected();
            }
        }
    });

    self.GetSelectedView = function (artifactId) {
        artifactId = artifactId || self.ViewId();

        var selectedView = ko.utils.arrayFirst(self.AvailableViews(), function (item) {
            if (item.artifactId === artifactId) {
                return item;
            }
        });

        return selectedView;
    };

    self.UpdateSelectedView = function (artifactId) {
        var selectedView = self.GetSelectedView(artifactId);

        if (!!selectedView) {
            self.ViewId(selectedView.artifactId);
        } else {
            self.FolderArtifactName(undefined);
            self.ViewId(undefined);
        }
    };

    self.UpdateViews = function (folderId, viewId) {
        self.Folders(self.Cache.ViewsResult[0]);
        self.AvailableViews(self.Cache.ViewsResult[1]);
        self.UpdateSelectedView(viewId || self.ViewId());
    };

    // productions

    self.ProductionSets = ko.observableArray();

    self.ProductionName = ko.observable(sourceConfiguration.ProductionName);

    self.IsProductionSelected = function () {
        return self.TypeOfExport() === ExportEnums.SourceOptionsEnum.Production;
    };

    self.ProductionId = ko.observable(sourceConfiguration.ProductionId).extend({
        required: {
            onlyIf: function () {
                return self.IsProductionSelected();
            }
        }
    });

    self.GetSelectedProduction = function (artifactId) {
        var selectedProduction = ko.utils.arrayFirst(self.ProductionSets(), function (item) {
            if (item.artifactID === artifactId) {
                return item;
            }
        });
        return selectedProduction;
    };

    self.UpdateSelectedProduction = function (artifactId) {
        var selectedProduction = self.GetSelectedProduction(artifactId);

        if (!!selectedProduction) {
            self.ProductionId(selectedProduction.artifactID);
            self.ProductionName(selectedProduction.displayName);
        } else {
            self.ProductionId(undefined);
            self.ProductionName(undefined);
        }
    };

    self.UpdateProductions = function (artifactId) {
        self.ProductionSets(self.Cache.ProductionsResult);
        self.UpdateSelectedProduction(artifactId || self.ProductionId());
    }

    // -----

    self.InitializeLocationSelector = function () {
        self.LocationSelector = new LocationJSTreeSelector();

        if (self.HasBeenRun()) {
            self.LocationSelector.toggle(false);
        } else {
            self.LocationSelector.init(self.FolderArtifactName(), [], {
                onNodeSelectedEventHandler: function (node) {
                    self.FolderArtifactName(node.text);
                    self.FolderArtifactId(node.id);
                }
            });
            self.LocationSelector.toggle(true);

            self.Folders.subscribe(function (value) {
                self.LocationSelector.reload(value);
            });

            var folders = self.Folders();
            if (folders !== undefined) {
                self.LocationSelector.reload(folders);
            }
        }
    };

    self.Reload = function () {
        switch (self.TypeOfExport()) {
            case ExportEnums.SourceOptionsEnum.Folder:
            case ExportEnums.SourceOptionsEnum.FolderSubfolder:
                if (typeof (self.Cache.ViewsResult) === 'undefined') {
                    var searchFoldersPromise = IP.data.ajax({
                        type: 'get',
                        url: IP.utils.generateWebAPIURL('SearchFolder/GetFolders')
                    }).fail(function (error) {
                        IP.message.error.raise("No folders were returned from the source provider.");
                    });

                    var viewsPromise = IP.data.ajax({
                        type: "get",
                        url: IP.utils.generateWebAPIURL("WorkspaceView/GetViews", 10) // TODO here 10 and is an artifactTypeId of Document it should be taken from the configuration
                    }).fail(function (error) {
                        IP.message.error.raise("No views were returned from the source provider.");
                    });

                    var currentFolderId = self.FolderArtifactId(),
                        currentViewId = self.ViewId();

                    IP.data.deferred().all([searchFoldersPromise, viewsPromise]).then(function (result) {
                        self.Cache.ViewsResult = result;
                        self.UpdateViews(currentFolderId, currentViewId);
                    });
                } else {
                    self.UpdateViews();
                }
                break;

            case ExportEnums.SourceOptionsEnum.Production:
                if (typeof (self.Cache.ProductionsResult) === 'undefined') {
                    var productionSetsPromise = IP.data.ajax({
                        type: "get",
                        url: IP.utils.generateWebAPIURL("Production/Productions"),
                        data: {
                            sourceWorkspaceArtifactId: IP.utils.getParameterByName("AppID", window.top)
                        }
                    }).fail(function (error) {
                        IP.message.error.raise("No production sets were returned from the source provider.");
                    });

                    var currentProductionId = self.ProductionId();

                    IP.data.deferred().all(productionSetsPromise).then(function (result) {
                        self.Cache.ProductionsResult = result;
                        self.UpdateProductions(currentProductionId);
                    });
                } else {
                    self.UpdateProductions();
                }
                break;

            case ExportEnums.SourceOptionsEnum.SavedSearch:
                if (typeof (self.Cache.SavedSearchesResult) === 'undefined') {
                    var savedSearchesTreePromise = IP.data.ajax({
                        type: 'get',
                        url: IP.utils.generateWebAPIURL('SavedSearchesTree', IP.utils.getParameterByName("AppID", window.top))
                    }).fail(function (error) {
                        IP.message.error.raise(error);
                    });

                    var currentSavedSearchArtifactId = self.SavedSearchArtifactId();

                    IP.data.deferred().all(savedSearchesTreePromise).then(function (result) {
                        self.Cache.SavedSearchesResult = result;
                        self.UpdateSavedSearches(currentSavedSearchArtifactId);
                    });
                } else {
                    self.UpdateSavedSearches();
                }
                break;
        }
    };
};