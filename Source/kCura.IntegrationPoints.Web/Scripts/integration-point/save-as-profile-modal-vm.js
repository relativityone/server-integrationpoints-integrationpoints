//formats the error message under the input box
ko.validation.insertValidationMessage = function (element) {
    var errorContainer = document.createElement('div');
    var iconSpan = document.createElement('span');
    iconSpan.className = 'icon-error legal-hold field-validation-error';

    errorContainer.appendChild(iconSpan);

    $(element).parents('.field-value').eq(0).append(errorContainer);

    return iconSpan;
};

var SaveAsProfileModalViewModel = function (okCallback) {
    var self = this;

    self.profileName = ko.observable().extend({
        required: true,
        textFieldWithoutSpecialCharacters: {}
    });

    self.okCallback = okCallback;
    self.data = {};

    self.view = null;

    this.construct = function (view) {
        self.view = view;
    }

    this.validate = function () {
        this.validationModel = ko.validatedObservable({
            profileName: this.profileName
        });

        return this.validationModel.isValid();
    }

    this.open = function (name) {
        self.profileName(name);
        self.view.dialog("open");
        self.view.keypress(function (e) {
            if (e.which === 13) {
                self.ok();
            }
        });
    }

    this.ok = function () {
        if (this.validate()) {
            self.okCallback(self.profileName());
            self.view.dialog("close");
        }
    }

    this.cancel = function () {
        self.view.dialog("close");
    }
}
