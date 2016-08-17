var FieldMappingViewModel = function () {
    var self = this;

    self.availableFields = ko.observableArray([]);
    self.selectedAvailableFields = ko.observableArray([]);

    self.mappedFields = ko.observableArray([]).extend({
        minLength: {
            params: 1,
            message: "Please select at least one field."
        }
    });
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
};
