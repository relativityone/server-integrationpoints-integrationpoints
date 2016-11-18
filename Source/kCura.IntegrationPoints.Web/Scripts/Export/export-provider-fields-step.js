﻿var IP = IP || {};

(function (root, ko) {
    var viewModel = function (state) {
        var self = this;

        self.HasBeenRun = ko.observable(state.hasBeenRun || false);

        self.fields = new FieldMappingViewModel();

        self.exportSource = new ExportSourceViewModel(state);
        self.exportSource.Reload();

        self.startExportAtRecord = ko.observable(state.sourceConfiguration.StartExportAtRecord || 1).extend({
            required: true,
            min: 1,
            nonNegativeNaturalNumber: {}
        });

        self.onDOMLoaded = function () {
            self.exportSource.InitializeLocationSelector();
        };
    };

    var stepModel = function (settings) {
        var self = this;

        self.settings = settings;
        self.template = ko.observable();
        self.hasTemplate = false;
        self.getTemplate = function () {
            root.data.ajax({
                dataType: 'html',
                cache: true,
                type: 'get',
                url: self.settings.url
            }).then(function (result) {
                $('body').append(result);
                self.hasTemplate = true;
                self.template(self.settings.templateID);
                self.model.onDOMLoaded();
                root.messaging.publish('details-loaded');
            });
        }

        self.ipModel = {};
        self.model = {};
        self.cache = {};

        self.loadModel = function (ip) {
            self.ipModel = ip;
            self.ipModel.SelectedOverwrite = "Append/Overlay"; // hardcoded as this value doesn't relate to export

            if (typeof ip.sourceConfiguration === "string") {
                try {
                    // parse config of existing IP
                    this.ipModel.sourceConfiguration = JSON.parse(ip.sourceConfiguration);
                } catch (e) {
                    // create new config
                    this.ipModel.sourceConfiguration = {
                        SourceWorkspaceArtifactId: IP.data.params['appID']
                    };
                }
            }

            self.model = new viewModel($.extend({}, self.ipModel, {
                hasBeenRun: ip.hasBeenRun,
                cache: self.cache
            }));

            self.model.errors = ko.validation.group(self.model);

            self.getAvailableFields = function (fieldName, fieldValue) {
                self.ipModel.sourceConfiguration[fieldName] = fieldValue || 0;
                self.ipModel.sourceConfiguration.ExportType = self.model.exportSource.TypeOfExport();

                if (!!fieldValue) {
                    root.data.ajax({
                        type: 'post',
                        url: root.utils.generateWebAPIURL('ExportFields/Available'),
                        data: JSON.stringify({
                            options: self.ipModel.sourceConfiguration,
                            type: self.ipModel.source.selectedType
                        })
                    }).then(function (result) {
                        self.model.fields.removeAllFields();
                        self.model.fields.selectedAvailableFields(ko.utils.arrayMap(result, function (_item1) {
                            var _field = ko.utils.arrayFilter(self.model.fields.availableFields(), function (_item2) {
                                return _item1.fieldIdentifier === _item2.fieldIdentifier;
                            });
                            return _field[0];
                        }));
                        self.model.fields.addField();
                    }).fail(function (error) {
                        IP.message.error.raise("No available fields were returned from the source provider.");
                    });
                } else {
                    self.model.fields.removeAllFields();
                }
            };

            self.model.exportSource.TypeOfExport.subscribe(function (value) {
                self.ipModel.sourceConfiguration.ExportType = (value === parseInt(value)) ? value : undefined;

                self.ipModel.sourceConfiguration.SavedSearchArtifactId = 0;
                self.ipModel.sourceConfiguration.ProductionId = 0;
                self.ipModel.sourceConfiguration.ProductionName = undefined;
                self.ipModel.sourceConfiguration.ViewId = 0;

                self.model.fields.removeAllFields();

                self.model.exportSource.Cache = self.cache = {};

                self.model.exportSource.SavedSearchArtifactId(undefined);
                self.model.exportSource.ProductionId(undefined);
                self.model.exportSource.ProductionName(undefined);
                self.model.exportSource.FolderArtifactName(undefined);
                self.model.exportSource.ViewId(undefined);

                self.model.exportSource.Reload();
            });

            var exportableFieldsPromise = root.data.ajax({
                type: 'post',
                url: root.utils.generateWebAPIURL('ExportFields/Exportable'),
                data: JSON.stringify({
                    options: self.ipModel.sourceConfiguration,
                    type: self.ipModel.source.selectedType
                })
            }).fail(function (error) {
                IP.message.error.raise("No exportable fields were returned from the source provider.");
            });

            var mappedFieldsPromise;
            if (self.ipModel.artifactID > 0) {
                mappedFieldsPromise = root.data.ajax({
                    type: 'get',
                    url: root.utils.generateWebAPIURL('FieldMap', self.ipModel.artifactID)
                }).fail(function (error) {
                    IP.message.error.raise("No mapped fields were returned from the source provider.");
                });
            } else if (!!self.ipModel.Map) {
                mappedFieldsPromise = self.ipModel.Map;
            } else {
                mappedFieldsPromise = [];
            }

            var getMappedFields = function (fields) {
                var _fields = ko.utils.arrayMap(fields, function (_item1) {
                    var _field = ko.utils.arrayFilter(self.model.fields.availableFields(), function (_item2) {
                        return (_item1.sourceField) ?
                            (_item2.fieldIdentifier === _item1.sourceField.fieldIdentifier) :
                            (_item2.fieldIdentifier === _item1.fieldIdentifier);
                    });
                    return _field[0];
                });
                return _fields;
            };

            root.data.deferred().all([exportableFieldsPromise, mappedFieldsPromise]).then(function (result) {
                self.model.fields.availableFields(result[0]);
                self.model.fields.selectedAvailableFields(getMappedFields(result[1]));
                self.model.fields.addField();

                self.model.exportSource.SavedSearchArtifactId.subscribe(function (value) {
                    self.getAvailableFields("SavedSearchArtifactId", value);
                });

                self.model.exportSource.ProductionId.subscribe(function (value) {
                    self.getAvailableFields("ProductionId", value);
                });

                self.model.exportSource.ViewId.subscribe(function (value) {
                    self.getAvailableFields("ViewId", value);
                });
            });
        }

        self.submit = function () {
            var d = root.data.deferred().defer();

            if (self.model.errors().length === 0) {
                // update integration point's model
                self.ipModel.sourceConfiguration.StartExportAtRecord = self.model.startExportAtRecord();

                switch (self.ipModel.sourceConfiguration.ExportType) {
                    case ExportEnums.SourceOptionsEnum.Folder:
                    case ExportEnums.SourceOptionsEnum.FolderSubfolder:
                        self.ipModel.sourceConfiguration.FolderArtifactId = self.model.exportSource.FolderArtifactId();
                        self.ipModel.sourceConfiguration.FolderArtifactName = self.model.exportSource.FolderArtifactName();
                        self.ipModel.sourceConfiguration.FolderFullName = self.model.exportSource.GetFolderFullName();

                        var selectedView = self.model.exportSource.GetSelectedView();
                        self.ipModel.sourceConfiguration.ViewId = selectedView.artifactId;
                        self.ipModel.sourceConfiguration.ViewName = selectedView.name;
                        break;

                    case ExportEnums.SourceOptionsEnum.Production:
                        self.ipModel.sourceConfiguration.ProductionId = self.model.exportSource.ProductionId();
                        self.ipModel.sourceConfiguration.ProductionName = self.model.exportSource.ProductionName();
                        break;

                    case ExportEnums.SourceOptionsEnum.SavedSearch:
                        var selectedSavedSearch = self.model.exportSource.GetSelectedSavedSearch(self.model.exportSource.SavedSearchArtifactId());
                        self.ipModel.sourceConfiguration.SavedSearchArtifactId = selectedSavedSearch.value;
                        self.ipModel.sourceConfiguration.SavedSearch = selectedSavedSearch.displayName;
                        break;
                }

                var fieldMap = [];
                var hasIdentifier = false;

                self.model.fields.mappedFields().forEach(function (e, i) {
                    fieldMap.push({
                        sourceField: {
                            displayName: e.displayName,
                            isIdentifier: e.isIdentifier,
                            fieldIdentifier: e.fieldIdentifier,
                            isRequired: e.isRequired
                        },
                        destinationField: {
                            displayName: e.displayName,
                            isIdentifier: e.isIdentifier,
                            fieldIdentifier: e.fieldIdentifier,
                            isRequired: e.isRequired
                        },
                        fieldMapType: e.isIdentifier ? "Identifier" : "None"
                    });
                });

                // we need to have an identifier field in order not to break export
                // based on sync worker which performs field mapping
                if (!hasIdentifier) {
                    fieldMap[0].sourceField.isIdentifier = true;
                    fieldMap[0].destinationField.isIdentifier = true;
                    fieldMap[0].fieldMapType = "Identifier";
                }

                self.ipModel.Map = fieldMap;

                Picker.closeDialog("savedSearchPicker");

                d.resolve(self.ipModel);
            } else {
                self.model.errors.showAllMessages();
                d.reject();
            }

            return d.promise;
        }

        self.back = function () {
            var d = root.data.deferred().defer();

            Picker.closeDialog("savedSearchPicker");

            d.resolve(self.ipModel);

            return d.promise;
        }
    };

    var step = new stepModel({
        url: IP.utils.generateWebURL('IntegrationPoints', 'ExportProviderFields'),
        templateID: 'exportProviderFieldsStep',
        isForRelativityExport: true
    });

    root.points.steps.push(step);
})(IP, ko);