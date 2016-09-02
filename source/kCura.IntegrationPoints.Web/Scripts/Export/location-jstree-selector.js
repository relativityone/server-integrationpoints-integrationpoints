var LocationJSTreeSelector = function () {
	var self = this;
	self.domSelectorSettings = {
		dropdownSelector : 'select#location-select',
		dropdownOptionSelectedSelector: 'select#location-select option:selected',
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
			$(self.domSelectorSettings.jstreeHolderDivSelector).width($(self.domSelectorSettings.dropdownSelector).outerWidth());
			$(self.domSelectorSettings.jstreeHolderDivSelector).show();
			self.treeVisible = true;
		} else {
			$(self.domSelectorSettings.jstreeHolderDivSelector).width($(self.domSelectorSettings.dropdownSelector).outerWidth());
			$(self.domSelectorSettings.jstreeHolderDivSelector).hide();
			self.treeVisible = false;
		}
	};

	self.clearSelection = function () {
		$(self.domSelectorSettings.dropdownSelector).empty();
		$(self.domSelectorSettings.dropdownSelector).prop('selectedIndex', 0);
		$(self.domSelectorSettings.dropdownOptionSelectedSelector).hide();
	};

	self.setSelection = function (newValue) {
		$(self.domSelectorSettings.dropdownSelector).empty();
		$(self.domSelectorSettings.dropdownSelector).append('<option>' + newValue + '</option>');
		$(self.domSelectorSettings.dropdownSelector).prop('selectedIndex', 0);
		$(self.domSelectorSettings.dropdownOptionSelectedSelector).hide();
	};

	return {
		init: self.init,
		reload: self.reload,
		SelectedNode: self.SelectedNode
	}

}
