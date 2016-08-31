var LocationJSTreeSelector = function () {
	var self = this;

	self.create = function (setterCallback) {
		$('select#location-select').mousedown(function () {
			if (self.treeVisible) {
				//self.setSelection(self.SelectedFolderPath);
			}
			self.setTreeVisibility(!self.treeVisible);
		});
		self.setterCallback = setterCallback;
		self.initJsTree();
		self.setTreeVisibility(self.treeVisible);
	};

	self.initJsTree = function () {
		$('div#browser-tree').jstree({
			'core': {
				'data': [{
					"text": "localhost",
					"id": "localhost",
					"children": [{
						"text": "Shared",
						"id": "localhost\\Shared"
					}, {
						"text": "Temp",
						"id": "localhost\\Temp"
					}]
				}]
			}
		});

		$('div#browser-tree').on('select_node.jstree', function (evt, data) {

			self.setSelection(data.node.id);
			self.setterCallback(data.node.id);
			self.SelectedFolderPath = data.node.text;
			self.setTreeVisibility(false);
		}
	  );
	};
	self.treeVisible = false;
	self.SelectedFolderPath = '';

	self.setTreeVisibility = function (visible) {
		if (visible) {
			$('#jstree-holder-div').width($('select#location-select').outerWidth());
			$('#jstree-holder-div').show();
			self.treeVisible = true;
		} else {
			$('#jstree-holder-div').width($('select#location-select').outerWidth());
			$('#jstree-holder-div').hide();
			self.treeVisible = false;
		}
	};

	self.clearSelection = function () {
		$('select#location-select').empty();
		$('select#location-select').prop('selectedIndex', 0);
		$('select#location-select option:selected').hide();
	};

	self.setSelection = function (newValue) {
		$('select#location-select').empty();
		$('select#location-select').append('<option>' + newValue + '</option>');
		$('select#location-select').prop('selectedIndex', 0);
		$('select#location-select option:selected').hide();
	};

}
