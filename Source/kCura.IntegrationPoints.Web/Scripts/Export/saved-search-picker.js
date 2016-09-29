var SavedSearchPickerViewModel = function (okCallback) {
	var self = this;

	self.PopupTitle = ko.observable("Select a Saved Search");

	self.okCallback = okCallback;
	self.data = {};

	self.view = null;

	this.construct = function (view) {
		self.view = view;
	}

	this.open = function (available, selected) {
		var $tree = $("#saved-search-picker-browser-tree");

		$tree.jstree("destroy");

		var _d = $.extend({ icon: "jstree-root-folder" }, available)
		$tree.jstree({
			core: {
				data: _d
			}
		});

		self.selected = selected;

		$tree.on("select_node.jstree", function (evt, data) {
			self.selected = data.node;
		});

		self.view.dialog('open');
	}

	this.ok = function () {
		self.okCallback(self.selected);
		self.view.dialog('close');
	}

	this.cancel = function () {
		self.view.dialog('close');
	}

	// this.init = function () {
	// }
}
