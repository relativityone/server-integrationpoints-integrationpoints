var LocationJSTreeSelector = function () {
	var self = this;
	self.domSelectorSettings = {
		dropdownSelector : 'div#location-select',
		inputSelector: 'input#location-input',
		browserTreeSelector : 'div#browser-tree',
		jstreeHolderDivSelector: '#jstree-holder-div',
		onNodeSelectedEventHandler: function () { }
	}
	self.treeVisible = false;
	self.SelectedNode = '';

	self.init = function (selectedNode, data, settings) {
		if (settings !== undefined) {
			$.extend(self.domSelectorSettings,settings);
		}

		$(self.domSelectorSettings.dropdownSelector).mousedown(function () {
			self.setTreeVisibility(!self.treeVisible);
		});
		
		if (selectedNode !== undefined) {
			self.SelectedNode = selectedNode ;
			self.setSelection(selectedNode);
		}
		
		self.initJsTree(data);
		self.setTreeVisibility(self.treeVisible);
	};

	self.reload = function (data) {
		self.initJsTree(data);
		self.setTreeVisibility(self.treeVisible);
	};

	self.initJsTree = function (data) {		
		$(self.domSelectorSettings.jstreeHolderDivSelector).width($(self.domSelectorSettings.dropdownSelector).innerWidth());

		$(self.domSelectorSettings.browserTreeSelector).jstree('destroy');
		var root = $.extend({ "icon": "jstree-root-folder" }, data);

		$(self.domSelectorSettings.browserTreeSelector).jstree({
			'core': {
				'data': root
			}
		});

		$(self.domSelectorSettings.browserTreeSelector).on('select_node.jstree', function (evt, data) {
			self.setSelection(data.node.id);
			self.SelectedNode = data.node.text;
			self.domSelectorSettings.onNodeSelectedEventHandler(data.node);
			self.setTreeVisibility(false);
		});
	};

	self.setTreeVisibility = function (visible) {
		if (visible) {
			$(self.domSelectorSettings.jstreeHolderDivSelector).show();
			self.treeVisible = true;
		} else {
			$(self.domSelectorSettings.jstreeHolderDivSelector).hide();
			self.treeVisible = false;
		}
	};

	self.clearSelection = function () {
		$(self.inputSelector).attr("value", "");
		self.reload(undefined);
	};

	self.setSelection = function (newValue) {
		$(self.inputSelector).attr("value", newValue);
	};

	self.toggleLocation = function (enabled) {
		var $el = $("#location-select");
		$el.toggleClass('location-disabled', !enabled);
		$el.children().each(function (i, e) {
			$(e).toggleClass('location-disabled', !enabled);
		});
	};

	return {
		init: self.init,
		reload: self.reload,
		clear: self.clearSelection,
		toggle: self.toggleLocation,
		SelectedNode: self.SelectedNode
	}

}
