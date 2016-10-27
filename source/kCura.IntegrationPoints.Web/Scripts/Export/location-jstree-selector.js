var LocationJSTreeSelector = function () {
	var self = this;
	self.domSelectorSettings = {
		dropdownSelector: 'div#location-select',
		inputSelector: 'input#location-input',
		browserTreeSelector: 'div#browser-tree',
		jstreeHolderDivSelector: '#jstree-holder-div',
		onNodeSelectedEventHandler: function () { }
	}
	self.treeVisible = false;
	self.SelectedNode = '';

	self.init = function (selectedNode, data, settings) {
		if (settings !== undefined) {
			$.extend(self.domSelectorSettings, settings);
		}

		$(self.domSelectorSettings.dropdownSelector).mousedown(function () {
			self.setTreeVisibility(!self.treeVisible);
		});

		if (selectedNode !== undefined) {
			self.SelectedNode = selectedNode;
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
		    //depending on if any optional settings are passed in, determine if only files or directories should be selectable
		    self.domSelectorSettings.selectFilesOnly = self.domSelectorSettings.selectFilesOnly || false;
		    if ((!self.domSelectorSettings.selectFilesOnly) || (!data.node.original.isDirectory && self.domSelectorSettings.selectFilesOnly)) {
		        self.setSelection(data.node.id);
		        self.SelectedNode = data.node.text;
		        self.domSelectorSettings.onNodeSelectedEventHandler(data.node);
		        self.setTreeVisibility(false);
		    }
		});
	};

	self.initWithRoot = function (selectedNode, ajaxCallback, settings) {
		if (settings !== undefined) {
			$.extend(self.domSelectorSettings, settings);
		}

		$(self.domSelectorSettings.dropdownSelector).mousedown(function () {
			self.setTreeVisibility(!self.treeVisible);
		});

		if (selectedNode !== undefined) {
			self.SelectedNode = selectedNode;
			self.setSelection(selectedNode);
		}

		self.initJsTreeWithRoot(ajaxCallback);
		self.setTreeVisibility(self.treeVisible);
	};

	self.initJsTreeWithRoot = function (ajaxCallback) {
		$(self.domSelectorSettings.jstreeHolderDivSelector).width($(self.domSelectorSettings.dropdownSelector).innerWidth());
		$(self.domSelectorSettings.browserTreeSelector).jstree('destroy');
		var extendWithDefault = function (child) {
			$.extend(child, { children: true });
			if (child.icon === null) {
				$.extend(child, { icon: "jstree-folder-default" });
			}
		};

		$(self.domSelectorSettings.browserTreeSelector).jstree({
			'core': {
				'data': function (obj, callback) {
					var ajaxSuccess = function (returnData) {
						$.each(returnData, function (index, value) {
							if (value.icon && value.icon === "jstree-root-folder") {
								$.each(value.children, function (index, child) {
									extendWithDefault(child);
								});
							}
							else
							{
								extendWithDefault(value);
							}
						});
						callback.call(this, returnData);
					};
					var ajaxFail = function (errorThrown) {
						console.log('JsTree load fail:');
						console.log(errorThrown);
					}
					ajaxCallback(obj, ajaxSuccess, ajaxFail);

				}
			}
		});

		$(self.domSelectorSettings.browserTreeSelector).on('select_node.jstree', function (evt, data) {
		    //depending on if any optional settings are passed in, determine if only files or directories should be selectable
		    self.domSelectorSettings.selectFilesOnly = self.domSelectorSettings.selectFilesOnly || false;
		    if ((!self.domSelectorSettings.selectFilesOnly) || (!data.node.original.isDirectory && self.domSelectorSettings.selectFilesOnly)) {
		        self.setSelection(data.node.id);
		        self.SelectedNode = data.node.text;
		        self.domSelectorSettings.onNodeSelectedEventHandler(data.node);
		        self.setTreeVisibility(false);
		    }
		});
	};

	self.reloadWithRoot = function (ajaxCallback) {
		self.initJsTreeWithRoot(ajaxCallback);
		self.setTreeVisibility(self.treeVisible);
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
		reloadWithRoot: self.reloadWithRoot,
		clear: self.clearSelection,
		toggle: self.toggleLocation,
		SelectedNode: self.SelectedNode
	}

}
