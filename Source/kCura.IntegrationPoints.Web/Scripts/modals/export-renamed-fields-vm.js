﻿var ExportRenamedFieldsViewModel = function (okCallback) {
	var self = this;

	self.PopupTitle = ko.observable("Rename Fields");

	self.selectedFields = ko.observableArray([]);
	self.selectedFieldId = ko.observable();

	self.fieldRenamedText = ko.pureComputed({
		read: function () {
			if (self.selectedFieldId() !== undefined) {
				var selField = self.selectedField(self.selectedFieldId());
				return selField.renamedText;
			}
			return "";
		}
			,
		write: function (item) {
			if (self.selectedFieldId() !== undefined) {
				var selField = self.selectedField(self.selectedFieldId());
				selField.renamedText = item;
			}
		},
		owner: self
	}).extend({ notify: 'always' });

	self.selectedField = function (selectedId) {
		return ko.utils.arrayFirst(self.selectedFields(), function (item) {
			return item.fieldIdentifier === selectedId;
		});
	};

	this.okCallback = okCallback;

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

	this.next = function () {
		var nextIndex = self.selectedFields().findIndex(x => x.fieldIdentifier === self.selectedFieldId()) + 1;
		if (nextIndex >= self.selectedFields().length) {
			nextIndex = 0;
		}

		var nextField = self.selectedFields()[nextIndex];
		self.selectedFieldId(nextField.fieldIdentifier);
	};

	this.cancel = function () {
		self.view.dialog("close");
	};
}