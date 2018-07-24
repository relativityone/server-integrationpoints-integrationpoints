var IP = IP || {};
IP.utils = IP.utils || {};
IP.utils.generateWebURL = IP.utils.generateWebURL || function () { return '' };
IP.utils.generateWebAPIURL = IP.utils.generateWebAPIURL || function () { return '' };
IP.utils.getParameterByName = IP.utils.getParameterByName || function () { return '9' };
IP.data = IP.data || {
    ajax: function () {
        return {
            then: function () { },
            fail: function () { }
        }
    },
    deferred: function () {
        return {
            all: function () {
                return { then: function () { return { fail: function() {} } } }
            }
        };
    }
};
IP.frameMessaging = IP.frameMessaging || function () { return { subscribe: () => { } } };
IP.messaging = IP.messaging || { subscribe: () => { } };
IP.points = IP.points || { steps: [] };

var ExportEnums = ExportEnums || {};
ExportEnums.ProductionPrecedenceTypeEnum = ExportEnums.ProductionPrecedenceTypeEnum || {};
ExportEnums.ProductionPrecedenceTypeEnum.Produced = !!ExportEnums.ProductionPrecedenceTypeEnum.Produced;

var ImageProductionPickerViewModel = function () { return {} };

var Picker = Picker || { create: function () { } };
var TooltipViewModel = TooltipViewModel || function() { return {} };
var TooltipDefs = TooltipDefs || { 
    RelativityProviderSettingsDetails: 1,
    RelativityProviderSettingsDetailsTitle: 2
};