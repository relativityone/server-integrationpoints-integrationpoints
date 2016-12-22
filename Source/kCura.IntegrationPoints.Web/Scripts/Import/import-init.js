'use strict';
(function (windowObj, root) {
    windowObj.RelativityImport = {};
    windowObj.RelativityImport.getCurrentIpFieldMapping = window.top.getCurrentIpFieldMapping;
    windowObj.RelativityImport.WorkspaceId = root.utils.getParameterByName('AppID', windowObj.top);
    windowObj.RelativityImport.ImportTypeEnum = {Document: 0,Image: 1,Production: 2};
})(this, IP);