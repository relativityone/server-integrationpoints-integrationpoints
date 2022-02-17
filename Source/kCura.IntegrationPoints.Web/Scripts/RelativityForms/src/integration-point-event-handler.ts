import { createConsole } from "./event-handlers/createConsole";
import { EventNames } from "./types/eventNames";
import { IConvenienceApi } from "./types/convenienceApi";
import { setFieldsValues } from "./event-handlers/hydrateLayout";
import { transformLayout } from "./event-handlers/transformLayout";

export default function (eventNames: typeof EventNames, convenienceApi: IConvenienceApi) {

    var eventHandlers = {};

    eventHandlers[eventNames.TRANSFORM_LAYOUT] = function (layoutData) { transformLayout(layoutData, convenienceApi) };
    eventHandlers[eventNames.HYDRATE_LAYOUT_COMPLETE] = function (layoutData) { setFieldsValues(layoutData, convenienceApi) };
    eventHandlers[eventNames.CREATE_CONSOLE] = createConsole(convenienceApi);
    eventHandlers[eventNames.UPDATE_CONSOLE] = createConsole(convenienceApi);

    return eventHandlers;
};