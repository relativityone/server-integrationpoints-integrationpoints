(function () {
const { suite, test, beforeEach } = intern.getInterface('tdd');
const { assert } = intern.getPlugin('chai');

suite('save-as-profile-modal-vm.js', () => {
    let callbackResult = '';
    let dialogResult = '';

    const okCallback = function(profileName){
        callbackResult = profileName;    
    };

    const view = {
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
        const profile = new SaveAsProfileModalViewModel(okCallback);
        profile.construct(view);
        profile.open(profileName);
        return profile;
    }

    test('Example good data 1', () => {
        const saveAsProfileModalViewModel = setUpProfileView("Example data 1");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, "Example data 1");
        assert.strictEqual(dialogResult, "close");
    });

    test('Example wrong ?? data 123', () => {      
        const saveAsProfileModalViewModel = setUpProfileView("Example wrong ?? data 123");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('N/A', () => {
        const saveAsProfileModalViewModel = setUpProfileView('');
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('null', () => {
        const saveAsProfileModalViewModel = setUpProfileView(null);
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, '');
        assert.strictEqual(dialogResult, "open");
    });

    test('Other good data 2', () => {
        const saveAsProfileModalViewModel = setUpProfileView("Other good data 2");
        saveAsProfileModalViewModel.ok();
        assert.strictEqual(callbackResult, "Other good data 2");
        assert.strictEqual(dialogResult, "close");
    });

});
})()