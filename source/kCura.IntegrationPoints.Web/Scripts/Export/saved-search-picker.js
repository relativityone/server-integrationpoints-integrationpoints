var SavedSearchPicker = {
    create: function (okCallback) {
        var view = (window.parent.$)('<div id="savedSearchPicker" style="padding: 0px;"></div>');
        var viewModel = new ViewModel(view, okCallback);

        IP.data.ajax({
            url: IP.utils.generateWebURL('Fileshare', 'SavedSearchPicker'),
            type: 'get',
            dataType: 'html'
        }).then(function (result) {
            SavedSearchPicker.createDialog(result, view, viewModel);
        });

        return viewModel;
    },
    createDialog: function (modalHTML, view, viewModel) {
        var $myWin = $(window);
        var popupPos = { my: 'center', at: 'center', of: $myWin[0] };
        var h = 500;
        var w = 300;
        view.append(modalHTML).dialog({
            autoOpen: false,
            modal: true,
            width: w,
            height: h,
            resizable: false,
            draggable: false,
            closeOnEscape: true,
            position: popupPos
        });

        setTimeout(function () {
            viewModel.construct();
            ko.applyBindings(viewModel, view.get()[0]);
        });
    }
};

var ViewModel = function (view, okCallback) {
    var self = this;

    this.frame = null;
    this.hasBeenRun = false;
    this.view = view;
    this.okCallback = okCallback;

    this.construct = function () {
        self.frame = window.parent.frames['SearchFolderTreeiFrame'];
        if (!self.frame) {
            throw 'Saved Search Picker not found';
        }

        var appId = IP.utils.getParameterByName('AppID', window.top);
        this.src = "/Relativity/Controls/SearchContainer/TreeViewSelector.aspx?ArtifactID=" + appId + "&amp;AppID=" + appId;
        setTimeout(self.run, 1000);
    }

    this.open = function (currentArtifact) {
        if (!self.hasBeenRun) {
            self.init();
        }
        self.view.dialog('open');
        var currentArtifactId = currentArtifact == null ? null : currentArtifact.value;
        self.selectedArtifactId = currentArtifactId;
        self.frame.selectNodeByValue(currentArtifactId);
    }

    this.ok = function () {
        self.okCallback(self.selectedArtifactId);
        self.view.dialog('close');
    }

    this.cancel = function () {
        self.view.dialog('close');
    }

    this.init = function () {
        self.frame.OnSelectedNodeChanged = function (sender, args) {
            self.selectedArtifactId = args.get_node().get_value();
        };

        self.view.removeClass('ui-dialog-content').prev().hide();

        self.hasBeenRun = true;
    }
}
