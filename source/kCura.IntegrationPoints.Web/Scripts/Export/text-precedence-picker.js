var TextPrecedencePickerViewModel = function (okCallback) {
    var self = this;

    this.okCallback = okCallback;

    this.construct = function (view) {
        this.view = view;
    }

    this.open = function (currentSelection) {
        self.view.dialog('open');
    }

    this.availableFields = ko.observable();

    this.mappedFields = ko.observable();

    this.selectedAvailableFields = ko.observable();

    this.selectedMappedFields = ko.observable();

    self.addField = function () { };

    self.addAllFields = function () { };

    self.removeField = function () { };

    self.removeAllFields = function () { };

    self.moveFieldTop = function () { };

    self.moveFieldUp = function () { };

    self.moveFieldDown = function () { };

    self.moveFieldBottom = function () { };
}
