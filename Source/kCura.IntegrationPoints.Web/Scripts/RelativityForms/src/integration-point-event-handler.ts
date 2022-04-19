import { createConsole } from "./event-handlers/createConsole";
import { EventNames } from "./types/eventNames";
import { IConvenienceApi } from "./types/convenienceApi";
import { setFieldsValues } from "./event-handlers/hydrateLayout";
import { transformLayout } from "./event-handlers/transformLayout";
import { updeteJobHistoryTable } from "./helpers/itemListModify";

export default function (eventNames: typeof EventNames, convenienceApi: IConvenienceApi) {

    var eventHandlers = {};

    let sourceConfiguration;
    let destinationConfiguration;

    eventHandlers[eventNames.TRANSFORM_LAYOUT] = function (layoutData) {
        let configurations = transformLayout(layoutData, convenienceApi, this.backingModelData);
        sourceConfiguration = configurations[0];
        destinationConfiguration = configurations[1];
        
    };
    eventHandlers[eventNames.HYDRATE_LAYOUT_COMPLETE] = function (layoutData) {
        setFieldsValues(layoutData, convenienceApi, sourceConfiguration, destinationConfiguration)
    };
    eventHandlers[eventNames.CREATE_CONSOLE] = createConsole(convenienceApi);
    eventHandlers[eventNames.UPDATE_CONSOLE] = createConsole(convenienceApi);

    eventHandlers[eventNames.ITEM_LIST_MODIFY_ACTIONS] = function (itemListActionsApi, itemListView) {
        try {
            let relativityWindow = convenienceApi.utilities.getRelativityPageBaseWindow();
            updeteJobHistoryTable(convenienceApi, relativityWindow.location.href, itemListActionsApi._workspaceId, itemListView.ObjectTypeID, itemListView.LinkViewID, itemListActionsApi._artifactId, itemListView.ConnectorFieldArtifactID);
        } catch (err) {
            console.log(err)
        }
    }

    eventHandlers[eventNames.ITEM_LIST_MODIFY_COLUMNS] = function (columnsApi, view) {
        try {
            columnsApi.removeColumn("Integration Point");
            columnsApi.removeColumn("Destination Instance");
        } catch (err) {
            console.log(err);
        }
        
    }

    return eventHandlers;
};