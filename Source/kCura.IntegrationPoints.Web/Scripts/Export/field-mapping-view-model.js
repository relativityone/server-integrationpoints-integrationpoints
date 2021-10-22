var FieldMappingViewModel = function (hasBeenRun) {
	var self = this;

	self.HasBeenRun = hasBeenRun;

	self.setTitle = function (options, item) {
		options.title = item.displayName;
	};

	self.availableFields = ko.observableArray([]);
	self.selectedAvailableFields = ko.observableArray([]);

	self.mappedFields = ko.observableArray([]).extend({
		minLength: {
			params: 1,
			message: "Please select at least one field."
		}
	});

	self.mappedFields.subscribe(function () {
		var isHidden = self.mappedFields().length <= 0 || self.HasBeenRun;
		self.isRenameButtonHidden(isHidden);
	});

	self.selectedMappedFields = ko.observableArray([]);

	var exportRenamedFieldsViewModel = new ExportRenamedFieldsViewModel(function (fields) {
		self.removeAllFields();
		self.selectedAvailableFields(fields);
		self.addField();
	});

	Picker.create("Modals", "export-renamed-fields-modal", "ExportRenamedFieldsView", exportRenamedFieldsViewModel);

	self.openRenamedFieldsModal = function () {
		//Previous solution was using Array.slice() but it performs shallow copy which is not suitable
		//here as self.mappedFields() is array of objects
		//We need to deep copy source array so it doesn't get modified on dialog 'Cancel'
		var copy = $.extend(true, [], self.mappedFields());
		exportRenamedFieldsViewModel.open(copy, self.selectedMappedFields());
	};

	self.isRenameButtonHidden = ko.observable(true);

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
		self.sortAvailableFieldsAsc();
	};

	self.removeAllFields = function () {
		IP.workspaceFieldsControls.add(
          self.mappedFields,
          self.mappedFields,
          self.availableFields
		);
		self.sortAvailableFieldsAsc();
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

	self.sortAvailableFieldsAsc = function () {
		self.availableFields.sort(function (item1, item2) {
			return item1.displayName > item2.displayName ? 1 : -1;
		});
	};

	self.getMappedFields = function () {
		var fieldMap = [];
		var hasIdentifier = false;

		self.mappedFields().forEach(function (e, i) {
			fieldMap.push({
				sourceField: {
					displayName: e.displayName,
					isIdentifier: e.isIdentifier,
					fieldIdentifier: e.fieldIdentifier,
					isRequired: e.isRequired
				},
				destinationField: {
					displayName: e.renamedText.trim().length > 0 ? e.renamedText : e.displayName,
					isIdentifier: e.isIdentifier,
					fieldIdentifier: e.fieldIdentifier,
					isRequired: e.isRequired
				},
				fieldMapType: e.isIdentifier ? "Identifier" : "None"
			});
		});

		// we need to have an identifier field in order not to break export
		// based on sync worker which performs field mapping
		if (!hasIdentifier && fieldMap.length > 0) {
			fieldMap[0].sourceField.isIdentifier = true;
			fieldMap[0].destinationField.isIdentifier = true;
			fieldMap[0].fieldMapType = "Identifier";
		}

		return fieldMap;
	};

	self.createRenamedFileds = function (availableFields, mappedFields) {
		for (var i = 0; i < availableFields.length; i++) {
			availableFields[i].renamedText = "";
			if (!!mappedFields && mappedFields.length > 0) {
				var foundElem = mappedFields.find(function(mappedField) {
					return mappedField.destinationField.fieldIdentifier === availableFields[i].fieldIdentifier;
				});

				if (!!foundElem && foundElem.sourceField.displayName.trim() !== foundElem.destinationField.displayName.trim()) {
					availableFields[i].renamedText = foundElem.destinationField.displayName;
				}
			}
		}
	}
};