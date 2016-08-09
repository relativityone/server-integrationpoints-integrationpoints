var TextPrecedencePickerViewModel = function (okCallback, data) {
    var self = this;

    self.data = data;

    this.okCallback = okCallback;

    this.construct = function (view) {
        self.view = view;
    }

    this.open = function (selectedFields) {
        self.loadAvailableFields(selectedFields);
        self.view.dialog('open');
    }

    this.loadAvailableFields = function (selectedFields) {
        IP.data.ajax({
            type: 'get',
            url: IP.utils.generateWebAPIURL('ExportFields/LongTextFields'),
            data: {
                sourceWorkspaceArtifactId: IP.utils.getParameterByName('AppID', window.top)
            }
        }).then(function (result) {
            self.availableFields(result);
            self.loadSelectedFields(selectedFields);
        }).fail(function (error) {
            IP.message.error.raise("No attributes were returned from the source provider.");
        });
    }

    this.loadSelectedFields = function (selectedFields) {
        self.mappedFields([]);

        var getMappedFields = function (fields) {
            var _fields = ko.utils.arrayMap(fields, function (_item1) {
                var _field = ko.utils.arrayFilter(self.availableFields(), function (_item2) {
                    return (_item1.sourceField) ?
                    (_item2.fieldIdentifier === _item1.sourceField.fieldIdentifier) :
                    (_item2.fieldIdentifier === _item1.fieldIdentifier);
                });
                return _field[0];
            });
            return _fields;
        };

        var mappedFields = getMappedFields(selectedFields);

        self.selectedAvailableFields(mappedFields);
        self.addField();
    }

    this.ok = function () {
        self.okCallback(self.mappedFields());
        self.view.dialog('close');
    }

    this.cancel = function () {
        self.view.dialog('close');
    }

    self.availableFields = ko.observableArray([]);
    self.selectedAvailableFields = ko.observableArray([]);

    self.mappedFields = ko.observableArray([]);
    self.selectedMappedFields = ko.observableArray([]);

    self.addField = function () {
        IP.workspaceFieldsControls.add(
          self.availableFields,
          self.selectedAvailableFields,
          self.mappedFields
        );
    };

    self.addAllFields = function () {
        IP.workspaceFieldsControls.add(
          self.availableFields,
          self.availableFields,
          self.mappedFields
        );
    };

    self.removeField = function () {
        IP.workspaceFieldsControls.add(
          self.mappedFields,
          self.selectedMappedFields,
          self.availableFields
        );
    };

    self.removeAllFields = function () {
        IP.workspaceFieldsControls.add(
          self.mappedFields,
          self.mappedFields,
          self.availableFields
        );
    };

    self.moveFieldTop = function () {
        IP.workspaceFieldsControls.moveTop(
          self.mappedFields,
          self.selectedMappedFields()
        );
    };

    self.moveFieldUp = function () {
        IP.workspaceFieldsControls.up(
          self.mappedFields,
          self.selectedMappedFields
        );
    };

    self.moveFieldDown = function () {
        IP.workspaceFieldsControls.down(
          self.mappedFields,
          self.selectedMappedFields
        );
    };

    self.moveFieldBottom = function () {
        IP.workspaceFieldsControls.moveBottom(
          self.mappedFields,
          self.selectedMappedFields()
        );
    };
}
