(function (windowObj) {
    //Resize frame and fix scroll
    //var configFrame = $('#configurationFrame', window.parent.document);
    //configFrame.css('height', '100%');
    //configFrame.parent().css('height', '99%'); //99% avoids a vertical scroll
    //$('body').css('overflow-x', 'hidden').css('overflow-y', 'auto');

    windowObj.import = {};
    windowObj.import.MetaHelper = function (name) {
        return $('meta[name=' + name + ']').attr('content');
    };
    windowObj.import.IPFrameMessagingLoadEvent = false;
    windowObj.import.view = {};
    windowObj.import.view.treeVisible = false;

    var baseUrl = windowObj.import.MetaHelper('base-url');

    windowObj.import.view.setTreeVisibility = function (visible) {
        if (visible) {
            $('#jstree-holder-div').width($('#location-select').outerWidth());
            $('#jstree-holder-div').show();
        } else {
            $('#jstree-holder-div').width($('#location-select').outerWidth());
            $('#jstree-holder-div').hide();
        }
        windowObj.import.view.treeVisible = visible;
    };

    windowObj.import.view.clearSelection = function () {
        $('#location-select').empty();
        $('#location-select').prop('selectedIndex', 0);
        $('#location-select option:selected').hide();
    };

    windowObj.import.view.setSelection = function (newValue) {
        $('#location-select').empty();
        $('#location-select').append('<option>' + newValue + '</option>');
        $('#location-select').prop('selectedIndex', 0);
        $('#location-select option:selected').hide();
    };

    $('#location-select').mousedown(function () {
        if (windowObj.import.SelectedFolderPath) {
            windowObj.import.view.setSelection(windowObj.import.SelectedFolderPath);
        } else {
            windowObj.import.view.setSelection('');
        }
        windowObj.import.view.setTreeVisibility(!windowObj.import.view.treeVisible);
    });

    $(windowObj).resize(function (evt) {
        $('#jstree-holder-div').width($('#location-select').outerWidth());
    });

    windowObj.import.initJsTree = function () {
        $('#browser-tree').jstree('destroy');
        windowObj.import.view.setTreeVisibility(false);

        $('#browser-tree').jstree({
            'core': {
                'data': function (obj, callback) {
                    var ajaxSuccess = function (returnData) {
                        callback.call(this, returnData);
                    };
                    var ajaxFail = function (jqXHR, textStatus, errorThrown) {
                        console.log('JsTree load fail:');
                        console.log(textStatus);
                        console.log(errorThrown);
                    }
                    $.ajax({
                        url: baseUrl + 'api/RemoteDirectory/GetDirectory',
                        dataType: 'json',
                        data: { '': obj.id },
                        method: 'POST',
                        success: ajaxSuccess,
                        error: ajaxFail
                    });
                }
            }
        });

        $('#browser-tree').on('select_node.jstree', function (evt, data) {
            if (data.node.original.IsDirectory) {
                data.instance.toggle_node(data.node);
            } else {
                windowObj.import.view.setSelection(data.node.id);
                windowObj.import.SelectedFolderPath = data.node.id;
                windowObj.import.view.setTreeVisibility(false);
            }
        });
    };

    windowObj.import.updateSources = function () {
        var ajaxSuccess = function (returnData) {
            var loadedSourceConfig = false;
            if (windowObj.import.StorageRoot == null || windowObj.import.StorageRoot == '') {
                windowObj.import.StorageRoot = returnData.Sources[0];
            }
            else {
                loadedSourceConfig = true;
            }
            if (returnData.Sources.length > 1) {
                $('#sourceDropdown').show();
                $('#processingSources').unbind().empty();
                var optionIndex = 0;
                for (var idx in returnData.Sources) {
                    var newOption = $('<option value="' + returnData.Sources[idx] + '">' + returnData.Sources[idx] + '</option>');
                    $('#processingSources').append(newOption);
                    if (optionIndex === 0 && !loadedSourceConfig) {
                        $('#processingSources').val(newOption);
                    }
                }
                $('#processingSources').change(function () {
                    var selected = $('#processingSources option:selected').text();
                    var changeSuccess = function (returnData) {
                        windowObj.import.StorageRoot = returnData.StorageRoot;
                        windowObj.import.initJsTree();
                        if (!loadedSourceConfig) {
                            windowObj.import.view.clearSelection();
                            windowObj.import.SelectedFolderPath = '';
                        }
                        loadedSourceConfig = false;
                    };
                    var changeFail = function (jqXHR, textStatus, errorThrown) {
                        console.log('Failed to change root location');
                        console.log(textStatus);
                        console.log(errorThrown);
                    };
                    $.ajax({
                        url: baseUrl + 'api/StorageRoot/SetStorageRoot',
                        dataType: 'json',
                        method: 'POST',
                        data: { '': selected },
                        success: changeSuccess,
                        error: changeFail
                    });
                });

                if (loadedSourceConfig) {
                    $('#processingSources').prop('value', windowObj.import.StorageRoot);
                    $('#processingSources').change();
                    windowObj.import.view.setSelection(windowObj.import.SelectedFolderPath);
                }

            } else {
                $('#sourceText').show();
                $('#downloadSourceText').text(returnData.Sources[0]);

                if (loadedSourceConfig) {
                    windowObj.import.view.setSelection(windowObj.import.SelectedFolderPath);
                }
            }
        };
        var ajaxFail = function (jqXHR, textStatus, errorThrown) {
            console.log('Failed to update sources dropdown');
            console.log(textStatus);
            console.log(errorThrown);
        };
        var waitForFrameMessagingLoadEvent = function (returnData) {
            if (!windowObj.import.IPFrameMessagingLoadEvent) {
                windowObj.setTimeout(waitForFrameMessagingLoadEvent, 200, returnData);
            } else {
                ajaxSuccess(returnData);
            }
        }
        $.ajax({
            url: baseUrl + 'api/StorageRoot/GetSourceLocations',
            dataType: 'json',
            method: 'GET',
            success: waitForFrameMessagingLoadEvent,
            error: ajaxFail
        });
    };

    windowObj.import.view.setTreeVisibility(false);
    $('#sourceDropdown').hide();
    $('#sourceText').hide();

    windowObj.import.updateSources();
    windowObj.import.initJsTree();
})(this);