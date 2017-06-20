var Picker = {
	create: function (controller, containerId, viewName, viewModel, options) {
		var view = (window.$)('<div id="' + containerId + '" style="padding: 0px;"></div>');

		var promise = IP.data.ajax({
			url: IP.utils.generateWebURL(controller, viewName),
			type: "get",
			dataType: "html"
		}).then(function (result) {
		    Picker.createDialog(controller, result, view, viewModel, options);
		});
		return promise;
	},
	createDialog: function (controller, modalHTML, view, viewModel, options) {
		var $myWin = $(window);

		var selectedOptions;
		if (options) {
			selectedOptions = options;
			options.position.of = $myWin[0];
		} else {
		    selectedOptions = Picker.getDefaultOptions(controller);
			selectedOptions.position.of = $myWin[0];
		}

		view.append(modalHTML).dialog(selectedOptions);

		viewModel.construct(view);
		view.removeClass("ui-dialog-content").prev().hide();
		ko.applyBindings(viewModel, view.get()[0]);

	},
	closeDialog: function (containerId) {
		$('#' + containerId).dialog('destroy').remove();
	},
	getDefaultOptions: function (controller) {
	    var options;

	    switch (controller) {
	    case "Tooltip":
	        options = {
	            autoOpen: false,
	            modal: false,
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
	        break;

	    default:
	        options = {
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
	        break;
	    }

	    return options;
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