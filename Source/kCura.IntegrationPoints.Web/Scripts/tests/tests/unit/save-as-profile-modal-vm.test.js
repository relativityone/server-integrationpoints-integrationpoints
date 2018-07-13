(function () {
const { suite, test, beforeEach } = intern.getInterface('tdd');
const { assert } = intern.getPlugin('chai');

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

    beforeEach(()=>{
        callbackResult = '';
        dialogResult = '';
    })

    function setUpProfileView(profileName){
        var profile = new SaveAsProfileModalViewModel(okCallback);
        profile.construct(view);
        profile.open(profileName);
        return profile;
    }

    test('Example good data 1', () => {
        var saveAsProfileModalViewModel = setUpProfileView("Example data 1");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, "Example data 1");
        assert.strictEqual(dialogResult, "close");
    });

    test('Example wrong ?? data 123', () => {      
        var saveAsProfileModalViewModel = setUpProfileView("Example wrong ?? data 123");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('N/A', () => {
        var saveAsProfileModalViewModel = setUpProfileView('');
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('null', () => {
        var saveAsProfileModalViewModel = setUpProfileView(null);
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('Other good data 2', () => {
        var saveAsProfileModalViewModel = setUpProfileView("Other good data 2");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, "Other good data 2");
        assert.strictEqual(dialogResult, "close");
    });

});
})()