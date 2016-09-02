﻿var Picker = {
	create: function (containerId, viewName, viewModel, options) {	
		var view = (window.$)('<div id="' + containerId + '" style="padding: 0px;"></div>');

		IP.data.ajax({
			url: IP.utils.generateWebURL("Fileshare", viewName),
			type: "get",
			dataType: "html"
		}).then(function (result) {
			Picker.createDialog(result, view, viewModel, options);
		});
	},
	createDialog: function (modalHTML, view, viewModel, options) {
		var $myWin = $(window);

		var selectedOptions;
		if (options) {
			selectedOptions = options;
			options.position.of = $myWin[0];
		} else {
			selectedOptions = Picker.getDefaultOptions();
			selectedOptions.position.of = $myWin[0];
		}

		view.append(modalHTML).dialog(selectedOptions);

		setTimeout(function () {
			viewModel.construct(view);
			view.removeClass("ui-dialog-content").prev().hide();
			ko.applyBindings(viewModel, view.get()[0]);
		});
	},
	closeDialog: function (containerId) {
		$('#' + containerId).dialog('destroy').remove();
	},
	getDefaultOptions: function () {
		return {
			autoOpen: false,
			modal: true,
			width: "auto",
			height: "auto",
			resizable: false,
			draggable: false,
			closeOnEscape: true,
			position: {
				my: "center",
				at: "center"
			}
		};
	}
};

//This is template for Pickers' ViewModel
var ViewModelBase = function (okCallback) {
	this.view = null;
	this.okCallback = okCallback;

	this.construct = function (view) {
		this.view = view;
	};
	this.open = function (currentSelection) {
		self.view.dialog("open");
	};
};