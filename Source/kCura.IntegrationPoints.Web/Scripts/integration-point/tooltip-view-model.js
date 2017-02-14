var TooltipViewModel = function (tooltips, tooltipTitle) {
	var self = this;

	self.tooltips = ko.observableArray(tooltips);
	self.tooltipTitle = ko.observable(tooltipTitle);

	this.view = null;

	this.construct = function (view) {
		this.view = view;
	};
	this.open = function (event) {

		var position = self.view.dialog('option', 'position');
		position.of = event.currentTarget;
		position.at = "right+5 top";
		position.my = "left top";
		position.collision = "flip flip";

		self.view.dialog('option', 'position', position);
		self.view.dialog("open");
	};

	this.ok = function () {
		self.view.dialog("close");
	};
};

