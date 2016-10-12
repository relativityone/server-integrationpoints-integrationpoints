'use strict';
(function (windowObj, root) {
    windowObj.RelativityImport = {};
    windowObj.RelativityImport.getCurrentIpFieldMapping = window.top.getCurrentIpFieldMapping;
    windowObj.RelativityImport.WorkspaceId = root.utils.getParameterByName('AppID', windowObj.top);
})(this, IP);