import { createConsole } from "./event-handlers/createConsole";
import { EventNames } from "./types/eventNames";
import { IConvenienceApi } from "./types/convenienceApi";
import { setFieldsValues } from "./event-handlers/hydrateLayout";
import { transformLayout } from "./event-handlers/transformLayout";

export default function (eventNames: typeof EventNames, convenienceApi: IConvenienceApi) {

    var eventHandlers = {};

    let sourceConfiguration;
    let destinationConfiguration;

    eventHandlers[eventNames.TRANSFORM_LAYOUT] = function (layoutData) {
        try {
            console.log(this.backingModelData);
            let configurations = transformLayout(layoutData, convenienceApi, this.backingModelData);
            sourceConfiguration = configurations[0];
            destinationConfiguration = configurations[1];
        } catch (err) {
            console.log(err);
        }
        
    };
    eventHandlers[eventNames.HYDRATE_LAYOUT_COMPLETE] = function (layoutData) { setFieldsValues(layoutData, convenienceApi, sourceConfiguration, destinationConfiguration) };
    eventHandlers[eventNames.CREATE_CONSOLE] = createConsole(convenienceApi);
    eventHandlers[eventNames.UPDATE_CONSOLE] = createConsole(convenienceApi);

    return eventHandlers;
};