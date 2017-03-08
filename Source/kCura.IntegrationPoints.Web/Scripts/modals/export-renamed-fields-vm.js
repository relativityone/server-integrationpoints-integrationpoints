var ExportRenamedFieldsViewModel = function (okCallback) {
	var self = this;

	self.PopupTitle = ko.observable("Rename Fields");

	self.selectedFields = ko.observableArray([]);
	self.fieldRenamedText = ko.observable();

	self.selectedFieldId = ko.observable();

	self.selectedFieldId.subscribe(function (selectedId) {
		var selField = self.selectedField(selectedId);
		if (!!selField) {
			self.fieldRenamedText(selField.renamedText);
		}
	});

	self.selectedField = function (selectedId) {
		return ko.utils.arrayFirst(self.selectedFields(), function (item) {
			return item.fieldIdentifier === selectedId;
		});
	};

	this.isValidName = function (name) {
		return !!name && name.trim() !== "";
	};

	this.okCallback = okCallback;

	this.update = function () {
		if (!!self.selectedFieldId && self.isValidName(self.fieldRenamedText())) {
			self.selectedField(self.selectedFieldId()).renamedText = self.fieldRenamedText();
		}
	};

	this.construct = function (view) {
		self.view = view;
	};
	this.open = function (selectedFields) {
		self.selectedFields(selectedFields);
		self.view.dialog("open");
	};

	this.ok = function () {
		self.okCallback(self.selectedFields());
		self.view.dialog("close");
	};
	this.cancel = function () {
		self.view.dialog("close");
	};
}