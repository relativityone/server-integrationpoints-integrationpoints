var LocationJSTreeSelector = function () {
	var self = this;
	self.initJsTree = function () {
		$('div#browser-tree').jstree({
			'core': {
				'data': [{
					"text": "Root node",
					"children": [{
						"text": "Child node 1"
					}, {
						"text": "Child node 2"
					}]
				}]
			}
		});

		$('div#browser-tree').on('select_node.jstree', function (evt, data) {

			self.setSelection(data.node.text);
			self.SelectedFolderPath = data.node.id;
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
