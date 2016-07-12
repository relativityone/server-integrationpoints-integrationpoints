var TextPrecedencePickerViewModel = function (okCallback) {
    var self = this;

    this.okCallback = okCallback;

    this.construct = function (view) {
        this.view = view;
    }

    this.open = function (currentSelection) {
        self.view.dialog('open');
    }
}
