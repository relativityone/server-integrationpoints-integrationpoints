var SavedSearchPickerViewModel = function (okCallback, validateCallback) {
	var self = this;

	self.PopupTitle = ko.observable("Select a Saved Search");

	self.okCallback = okCallback;
	self.validateCallback = validateCallback;
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

		$(".jstree").on("loaded.jstree", function () {

			$('#saved-search-picker-browser-tree').jstree(true).select_node(self.selected);
		});


		self.view.dialog('open');
	}

	this.ok = function () {
		var canClose = true;

		if (typeof self.validateCallback === 'function') {
			canClose = self.validateCallback(self.selected);
		}

		if (canClose) {
			self.okCallback(self.selected);
			self.view.dialog('close');
		}
	}

	this.cancel = function () {
		self.view.dialog('close');
	}
}
