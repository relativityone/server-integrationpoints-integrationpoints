var { suite, test } = intern.getInterface('tdd');
var { assert } = intern.getPlugin('chai');

suite('save-as-profile-modal-vm.js', () => {
    var callbackResult = '';
    var dialogResult = '';
    var okCallback = function(profileName){
        callbackResult = profileName;    
    };
    var view = {
        dialog: function (dialogMsg) {
            dialogResult = dialogMsg;
        },
        keypress: function(){}
    };
    test('Example good data 1', () => {
        callbackResult = '';
        var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(okCallback);
        saveAsProfileModalViewModel.construct(view);
        saveAsProfileModalViewModel.open("Example data 1");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, "Example data 1");
        assert.strictEqual(dialogResult, "close");
    });

    test('Example wrong ?? data 123', () => {
        callbackResult = '';
        var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(okCallback);
        saveAsProfileModalViewModel.construct(view);
        saveAsProfileModalViewModel.open("Example wrong ?? data 123");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('N/A', () => {
        callbackResult = '';
        var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(okCallback);
        saveAsProfileModalViewModel.construct(view);
        saveAsProfileModalViewModel.open("");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('null', () => {
        callbackResult = '';
        var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(okCallback);
        saveAsProfileModalViewModel.construct(view);
        saveAsProfileModalViewModel.open(null);
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('Other good data 2', () => {
        callbackResult = '';
        var saveAsProfileModalViewModel = new SaveAsProfileModalViewModel(okCallback);
        saveAsProfileModalViewModel.construct(view);
        saveAsProfileModalViewModel.open("Other good data 2");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, "Other good data 2");
        assert.strictEqual(dialogResult, "close");
    });

})