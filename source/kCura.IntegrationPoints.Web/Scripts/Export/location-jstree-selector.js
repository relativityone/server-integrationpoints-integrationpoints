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

		$(self.domSelectorSettings.dropdownSelector).click(function (e) {
			e.stopPropagation();
		});

		$(self.domSelectorSettings.jstreeHolderDivSelector).click(function (e) {
			e.stopPropagation();
		});

		$(document).click(function () {
			self.setTreeVisibility(false);
		});

		if (selectedNode !== undefined) {
			self.SelectedNode = selectedNode;
			self.setSelection(selectedNode);
		}

		self.initJsTree(data, false);
		self.setTreeVisibility(self.treeVisible);
	};

	self.reload = function (data) {
		self.initJsTree(data, false);
		self.setTreeVisibility(self.treeVisible);
	};

	self.reloadWithRootWithData = function (data) {
		self.initJsTree(data, true);
		self.setTreeVisibility(self.treeVisible);
	};

	self.initJsTree = function (data, openRoot) {
		$(self.domSelectorSettings.jstreeHolderDivSelector).width($(self.domSelectorSettings.dropdownSelector).innerWidth());

		$(self.domSelectorSettings.browserTreeSelector).jstree('destroy');
		var root = $.extend({ "icon": "jstree-root-folder" }, data);

		$(self.domSelectorSettings.browserTreeSelector).jstree({
			'core': {
				'data': function (obj, callback) {
					if (openRoot) {
						self.openRootNode(self.domSelectorSettings.browserTreeSelector);
					}
					callback.call(this, root);
				}
			}
		});

		$(self.domSelectorSettings.browserTreeSelector).on('select_node.jstree', function (evt, data) {
			//depending on if any optional settings are passed in, determine if only files or directories should be selectable
			self.domSelectorSettings.selectFilesOnly = self.domSelectorSettings.selectFilesOnly || false;
			if ((!self.domSelectorSettings.selectFilesOnly) || (!data.node.original.isDirectory && self.domSelectorSettings.selectFilesOnly)) {
				self.setSelection(data.node.id);
				self.SelectedNode = data.node.text;
				data.node.fullPath = data.instance.get_path(data.node, '/');
				self.domSelectorSettings.onNodeSelectedEventHandler(data.node);
				self.setTreeVisibility(false);
			}
		});
	};

	self.openRootNode = function(treeSelector) {
		$(treeSelector).on('ready.jstree', function () {
			$(treeSelector).jstree('open_node', 'ul > li:first');
		});
	}

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
						//will open the root folder in jstree for both export and import
						$(self.domSelectorSettings.browserTreeSelector).on('ready.jstree', function () {
							$(self.domSelectorSettings.browserTreeSelector).jstree('select_node', 'ul > li:first');
							var selectedNode = $(self.domSelectorSettings.browserTreeSelector).jstree("get_selected");
							$(self.domSelectorSettings.browserTreeSelector).jstree("open_node", selectedNode, false, true);
						});
						$.each(returnData, function (index, value) {
							if (value.icon && value.icon === "jstree-root-folder") {
								$.each(value.children, function (index, child) {
									if (child.isDirectory == false) {
										//make sure that files don't have the expand button
										child.children = false;
									} else {
										extendWithDefault(child);
									}
								});
							} else if (value.icon && value.icon === "jstree-file") {
								//makeing sure to check nested folders to remove expand button
								value.children = false;
							} else {
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
			//allows for the jsTree for import to expand on folder click
			if (self.domSelectorSettings.selectFilesOnly) {
				data.instance.toggle_node(data.node);
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
		reloadWithRootWithData: self.reloadWithRootWithData,
		reloadWithRoot: self.reloadWithRoot,
		clear: self.clearSelection,
		toggle: self.toggleLocation,
		SelectedNode: self.SelectedNode
	}

}
