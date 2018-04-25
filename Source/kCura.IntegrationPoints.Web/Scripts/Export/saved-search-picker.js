var SavedSearchPickerViewModel = function (okCallback, retrieveDataCallback) {
	var self = this;

	self.PopupTitle = ko.observable("Select a Saved Search");

	self.okCallback = okCallback;
	self.isSavedSearchNode = function (node) {
		return !!node && (node.icon === "jstree-search" || node.icon === "jstree-search-personal");
	};
	self.data = {};

	self.view = null;

	self.retrieveData = function (node, callback) {
		var nodeId = null;
		if (node.id !== '#') {
			nodeId = node.id;
		}
		retrieveDataCallback(nodeId, callback);
	};

	this.construct = function (view) {
		self.view = view;
	}

	this.open = function (selected) {
		var $tree = $("#saved-search-picker-browser-tree");

		$tree.jstree("destroy");

		$tree.jstree({
			core: {
				data: self.retrieveData
			}
		});

		self.selected = selected;

		$tree.on("select_node.jstree", function (evt, data) {
			self.selected = data.node;
		});

		$(".jstree").on("loaded.jstree", function () {
			if (self.selected == undefined) {
				$('#saved-search-picker-browser-tree').jstree('open_node', 'ul > li:first'); // open root node
			} else {
				$('#saved-search-picker-browser-tree').jstree(true).select_node(self.selected); // open selected node
			}
		});
		
		self.view.dialog('open');
	}

	this.ok = function () {
		var canClose = true;

		if (typeof self.isSavedSearchNode === 'function') {
			canClose = self.isSavedSearchNode(self.selected);
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
