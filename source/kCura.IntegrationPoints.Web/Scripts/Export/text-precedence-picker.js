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
            self.model.availableFields(result);
            self.loadSelectedFields(selectedFields);
        }).fail(function (error) {
            IP.message.error.raise("No attributes were returned from the source provider.");
        });
    }

    this.loadSelectedFields = function (selectedFields) {
        self.model.mappedFields([]);

        var getMappedFields = function (fields) {
            var _fields = ko.utils.arrayMap(fields, function (_item1) {
                var _field = ko.utils.arrayFilter(self.model.availableFields(), function (_item2) {
                    return (_item1.sourceField) ?
                    (_item2.fieldIdentifier === _item1.sourceField.fieldIdentifier) :
                    (_item2.fieldIdentifier === _item1.fieldIdentifier);
                });
                return _field[0];
            });
            return _fields;
        };

        var mappedFields = getMappedFields(selectedFields);

        self.model.selectedAvailableFields(mappedFields);
        self.model.addField();
    }

    this.ok = function () {
        self.okCallback(self.model.mappedFields());
        self.view.dialog('close');
    }

    this.cancel = function () {
        self.view.dialog('close');
    }

    self.model = new FieldMappingViewModel();
}
