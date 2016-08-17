var SavedSearchPickerViewModel = function (okCallback) {
    var self = this;

    this.okCallback = okCallback;

    this.frame = null;
    this.view = null;

    this.hasBeenRun = false;

    this.construct = function (view) {
        self.view = view;
        self.frame = window.parent.frames['SearchFolderTreeiFrame'];
        if (!self.frame) {
            throw 'Saved Search Picker not found';
        }

        var appId = IP.utils.getParameterByName('AppID', window.top);
        this.src = "/Relativity/Controls/SearchContainer/TreeViewSelector.aspx?ArtifactID=" + appId + "&amp;AppID=" + appId;
    }

    this.open = function (currentSelection) {
        if (!self.hasBeenRun) {
            self.init();
        }
        self.view.dialog('open');
        var currentArtifactId = currentSelection == null ? null : currentSelection.value;
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
        self.hasBeenRun = true;
    }
}
